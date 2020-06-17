using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
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
        AssertEqual(0, "0;");
        AssertEqual(42, "42;");
    }

    [Test]
    public void テスト加減算()
    {
        AssertEqual(21, "5+20-4;");
        AssertEqual(41, "12 + 34 - 5;");
    }

    [Test]
    public void テスト四則演算()
    {
        AssertEqual(47, "5+6*7;");
        AssertEqual(15, "5*(9-6);");
        AssertEqual(4, "(3+5)/2;");
    }

    [Test]
    public void テスト単項演算子()
    {
        AssertEqual(10, "-10+20;");
        // AssertEqual(10, "- -10;");
        // AssertEqual(10, "- - +10;");
    }

    [Test]
    public void テスト比較演算子1()
    {
        AssertEqual(0, "0==1;");
        AssertEqual(1, "42==42;");
        AssertEqual(1, "0!=1;");
        AssertEqual(0, "42!=42;");
    }

    [Test]
    public void テスト比較演算子2()
    {
        AssertEqual(1, "0<1;");
        AssertEqual(0, "1<1;");
        AssertEqual(0, "2<1;");
        AssertEqual(1, "0<=1;");
        AssertEqual(1, "1<=1;");
        AssertEqual(0, "2<=1;");
    }

    [Test]
    public void テスト比較演算子3()
    {
        AssertEqual(1, "1>0;");
        AssertEqual(0, "1>1;");
        AssertEqual(0, "1>2;");
        AssertEqual(1, "1>=0;");
        AssertEqual(1, "1>=1;");
        AssertEqual(0, "1>=2;");
    }

    [Test]
    public void テストセミコロン()
    {
        AssertEqual(3, "1;2;3;");
    }

    [Test]
    public void テスト代入()
    {
        AssertEqual(3, "a=3;");
        AssertEqual(2, "ab=2;");
    }

    [Test]
    public void テスト複数変数()
    {
        AssertEqual(3, "foo=2;bar=1;foo+bar;");
        AssertEqual(3, "foo=4;bar=2;(foo+bar)/2;");
        AssertEqual(3, "foo=1;bar=2;foo=4;(foo+bar)/2;");
    }

    private void AssertEqual(int expect, string code)
    {
        var path = Write(Compiler.Compile(code));
        Compile(path);
        Assert.AreEqual(expect, Execute(path));
    }

    private string Write(string asm)
    {
        var path = Path.GetTempPath();
        File.WriteAllText(Path.Combine(path, OutputFileName), asm, Encoding.UTF8);

        return path;
    }

    private void Compile(string path)
    {
        var info = new ProcessStartInfo("cc", $"-o {OutputExeFileName} {OutputFileName}");

        info.WorkingDirectory = path;

        using (var proc = Process.Start(info))
        {
            proc.WaitForExit();
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
