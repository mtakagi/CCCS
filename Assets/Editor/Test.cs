﻿using NUnit.Framework;
using System.Text;
using System.IO;
using System.Diagnostics;
using CCCS;

public class CCCSTest
{
    private static string OutputFileName = "test.s";
    private static string OutputExeFileName = "test";

    [Test]
    public void テスト42()
    {
        AssertEqual(0, "return 0;");
        AssertEqual(42, "return 42;");
    }

    [Test]
    public void テスト加減算()
    {
        AssertEqual(21, "return 5+20-4;");
        AssertEqual(41, "return 12 + 34 - 5;");
    }

    [Test]
    public void テスト四則演算()
    {
        AssertEqual(47, "return 5+6*7;");
        AssertEqual(15, "return 5*(9-6);");
        AssertEqual(4, "return (3+5)/2;");
    }

    [Test]
    public void テスト単項演算子()
    {
        AssertEqual(10, "return -10+20;");
        // AssertEqual(10, "- -10;");
        // AssertEqual(10, "- - +10;");
    }

    [Test]
    public void テスト比較演算子1()
    {
        AssertEqual(0, "return 0==1;");
        AssertEqual(1, "return 42==42;");
        AssertEqual(1, "return 0!=1;");
        AssertEqual(0, "return 42!=42;");
    }

    [Test]
    public void テスト比較演算子2()
    {
        AssertEqual(1, "return 0<1;");
        AssertEqual(0, "return 1<1;");
        AssertEqual(0, "return 2<1;");
        AssertEqual(1, "return 0<=1;");
        AssertEqual(1, "return 1<=1;");
        AssertEqual(0, "return 2<=1;");
    }

    [Test]
    public void テスト比較演算子3()
    {
        AssertEqual(1, "return 1>0;");
        AssertEqual(0, "return 1>1;");
        AssertEqual(0, "return 1>2;");
        AssertEqual(1, "return 1>=0;");
        AssertEqual(1, "return 1>=1;");
        AssertEqual(0, "return 1>=2;");
    }

    [Test]
    public void テストセミコロン()
    {
        AssertEqual(3, "1;2;return 3;");
    }

    [Test]
    public void テスト代入()
    {
        AssertEqual(3, "a=3;return a;");
        AssertEqual(2, "ab=2;return ab;");
    }

    [Test]
    public void テスト複数変数()
    {
        AssertEqual(3, "foo=2;bar=1;return foo+bar;");
        AssertEqual(3, "foo=4;bar=2;return (foo+bar)/2;");
        AssertEqual(3, "foo=1;bar=2;foo=4;return (foo+bar)/2;");
    }

    [Test]
    public void テストIF文()
    {
        AssertEqual(3, "if (0) return 2; return 3;");
        AssertEqual(3, "if (1-1) return 2; return 3;");
        AssertEqual(2, "if (1) return 2; return 3;");
        AssertEqual(2, "if (2-1) return 2; return 3;");
    }

    [Test]
    public void テストwhile文()
    {
        AssertEqual(10, "i = 0; while(i < 10) i = i + 1; return i;");
    }

    [Test]
    public void テストfor文()
    {
        AssertEqual(55, "i=0; j=0; for (i=0; i<=10; i=i+1) j=i+j; return j;");
        AssertEqual(3, "for (;;) return 3; return 5;");
    }

    [Test]
    public void テストブロック()
    {
        AssertEqual(3, "{1; {2;} return 3;}");
        AssertEqual(55, "i=0; j=0; while (i<=10) {j=i+j; i=i+1;} return j;");
        AssertEqual(10, "i = 0; while(i < 10) {i = i + 1;} return i;");
    }

    [Test]
    public void テストFuncCall()
    {
        AssertEqual(3, "return ret3();");
        AssertEqual(5, "return ret5();");
    }

    [Test]
    public void テストFuncArg()
    {
        AssertEqual(8, "return add(3, 5);");
        AssertEqual(2, "return sub(5, 3);");
        AssertEqual(21, "return add6(1,2,3,4,5,6);");
    }

    private void AssertEqual(int expect, string code)
    {
        var path = Write(Compiler.Compile(code));
        Compile(path);
        Assert.AreEqual(expect, Execute(path));
    }

    private void BuildLib(string path)
    {
        var code =
@"int ret3() { return 3; }
int ret5() { return 5; }
int add(int x, int y) { return x+y; }
int sub(int x, int y) { return x-y; }
int add6(int a, int b, int c, int d, int e, int f) {
    return a+b+c+d+e+f;
}";
        var filePath = Path.Combine(path, "lib.c");
        File.WriteAllText(filePath, code, Encoding.UTF8);
        var info = new ProcessStartInfo("cc", "-c -o lib.o lib.c");

        info.UseShellExecute = false;
        info.WorkingDirectory = path;

        using (var proc = Process.Start(info))
        {
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                throw new System.Exception();
            }
        }
    }

    private string Write(string asm)
    {
        var path = Path.GetTempPath();
        File.WriteAllText(Path.Combine(path, OutputFileName), asm, Encoding.UTF8);

        return path;
    }

    private void Compile(string path)
    {
        BuildLib(path);
        var info = new ProcessStartInfo("cc", $"-o {OutputExeFileName} {OutputFileName} lib.o");

        info.WorkingDirectory = path;

        using (var proc = Process.Start(info))
        {
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                throw new System.Exception();
            }
        }
    }

    private int Execute(string path)
    {
        using (var proc = Process.Start(Path.Combine(path, OutputExeFileName)))
        {
            proc.WaitForExit();
            return proc.ExitCode;
        }
    }
}
