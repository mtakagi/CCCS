using NUnit.Framework;
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
        AssertEqual(0, "main() {return 0;}");
        AssertEqual(42, "main() {return 42;}");
    }

    [Test]
    public void テスト加減算()
    {
        AssertEqual(21, "main() {return 5+20-4;}");
        AssertEqual(41, "main() {return 12 + 34 - 5;}");
    }

    [Test]
    public void テスト四則演算()
    {
        AssertEqual(47, "main() {return 5+6*7;}");
        AssertEqual(15, "main() {return 5*(9-6);}");
        AssertEqual(4, "main() {return (3+5)/2;}");
    }

    [Test]
    public void テスト単項演算子()
    {
        AssertEqual(10, "main() {return -10+20;}");
        // AssertEqual(10, "- -10;");
        // AssertEqual(10, "- - +10;");
    }

    [Test]
    public void テスト比較演算子1()
    {
        AssertEqual(0, "main() {return 0==1;}");
        AssertEqual(1, "main() {return 42==42;}");
        AssertEqual(1, "main() {return 0!=1;}");
        AssertEqual(0, "main() {return 42!=42;}");
    }

    [Test]
    public void テスト比較演算子2()
    {
        AssertEqual(1, "main() {return 0<1;}");
        AssertEqual(0, "main() {return 1<1;}");
        AssertEqual(0, "main() {return 2<1;}");
        AssertEqual(1, "main() {return 0<=1;}");
        AssertEqual(1, "main() {return 1<=1;}");
        AssertEqual(0, "main() {return 2<=1;}");
    }

    [Test]
    public void テスト比較演算子3()
    {
        AssertEqual(1, "main() {return 1>0;}");
        AssertEqual(0, "main() {return 1>1;}");
        AssertEqual(0, "main() {return 1>2;}");
        AssertEqual(1, "main() {return 1>=0;}");
        AssertEqual(1, "main() {return 1>=1;}");
        AssertEqual(0, "main() {return 1>=2;}");
    }

    [Test]
    public void テストセミコロン()
    {
        AssertEqual(3, "main() {1;2;return 3;}");
    }

    [Test]
    public void テスト代入()
    {
        AssertEqual(3, "main() {a=3;return a;}");
        AssertEqual(2, "main() {ab=2;return ab;}");
    }

    [Test]
    public void テスト複数変数()
    {
        AssertEqual(3, "main() {foo=2;bar=1;return foo+bar;}");
        AssertEqual(3, "main() {foo=4;bar=2;return (foo+bar)/2;}");
        AssertEqual(3, "main() {foo=1;bar=2;foo=4;return (foo+bar)/2;}");
    }

    [Test]
    public void テストIF文()
    {
        AssertEqual(3, "main() {if (0) return 2; return 3;}");
        AssertEqual(3, "main() {if (1-1) return 2; return 3;}");
        AssertEqual(2, "main() {if (1) return 2; return 3;}");
        AssertEqual(2, "main() {if (2-1) return 2; return 3;}");
    }

    [Test]
    public void テストwhile文()
    {
        AssertEqual(10, "main() {i = 0; while(i < 10) i = i + 1; return i;}");
    }

    [Test]
    public void テストfor文()
    {
        AssertEqual(55, "main() {i=0; j=0; for (i=0; i<=10; i=i+1) {j=i+j;} return j;}");
        AssertEqual(3, "main() {for (;;) return 3; return 5;}");
    }

    [Test]
    public void テストブロック()
    {
        AssertEqual(3, "main() {{1; {2;} return 3;}}");
        AssertEqual(55, "main() {i=0; j=0; while (i<=10) {j=i+j; i=i+1;} return j;}");
        AssertEqual(10, "main() {i = 0; while(i < 10) {i = i + 1;} return i;}");
    }

    [Test]
    public void テストFuncCall()
    {
        AssertEqual(3, "main() {return ret3();}");
        AssertEqual(5, "main() {return ret5();}");
    }

    [Test]
    public void テストFuncArg()
    {
        AssertEqual(8, "main() {return add(3, 5);}");
        AssertEqual(2, "main() {return sub(5, 3);}");
        AssertEqual(21, "main() {return add6(1,2,3,4,5,6);}");
    }

    [Test]
    public void テストFunc定義()
    {
        AssertEqual(32, "main() { return ret32(); } ret32() { return 32; }");
    }

    [Test]
    public void テストFuncWithArg定義()
    {
        AssertEqual(7, "main() { return add2(3,4); } add2(x,y) { return x+y; }");
        AssertEqual(1, "main() { return sub2(4,3); } sub2(x,y) { return x-y; }");
        AssertEqual(21, "main() { return plus6(1, 2, 3, 4, 5, 6); } plus6(a, b, c, d, e, f) { return a+b+c+d+e+f; }");
        AssertEqual(55, "main() { return fib(9); } fib(x) { if (x<=1) return 1; return fib(x-1) + fib(x-2); }");
    }

    [Test]
    public void テストTokenizeException()
    {
        var e = Assert.Throws<TokenizeException>(
            () =>
            {
                var code = "main() {return 0;あ}";
                Compiler.Compile(code);
            }
        );

        Assert.NotNull(e);
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
