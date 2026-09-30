using System.Text;
using Hash.Hashing;

namespace Hash.Tests;

public class HashFuncTests
{
    private readonly HashFunc _hashFunc = new();
    private readonly HashaMasha _hashaMasha = new();

    [Fact]
    public void EmptyInputReturnsValidHash()
    {
        string hashI = _hashFunc.ComputeHash(Array.Empty<byte>());
        string hashA = _hashaMasha.ComputeHash(Array.Empty<byte>());
        AssertValidHash(hashI);
        AssertValidHash(hashA);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(65)]
    [InlineData(255)]
    public void SingleByteInputReturnsValidHash(int value)
    {
        string hashI = _hashFunc.ComputeHash(new[] { (byte)value });
        string hashA = _hashaMasha.ComputeHash(new[] { (byte)value });
        AssertValidHash(hashI);
        AssertValidHash(hashA);
    }

    [Fact]
    public void SameInputReturnsSameHash()
    {
        byte[] input = Encoding.UTF8.GetBytes("Hello World");

        string firstI = _hashFunc.ComputeHash(input);
        string secondI = _hashFunc.ComputeHash(input);
        string firstA = _hashaMasha.ComputeHash(input);
        string secondA = _hashaMasha.ComputeHash(input);

        Assert.Equal(firstI, secondI);
        Assert.Equal(firstA, secondA);
    }

    [Fact]
    public void SequenceABAReturnsSameHashForBothAValues()
    {
        byte[] a = Encoding.UTF8.GetBytes("A");
        byte[] b = Encoding.UTF8.GetBytes("B");

        string firstAI = _hashFunc.ComputeHash(a);
        _hashFunc.ComputeHash(b);
        string secondAI = _hashFunc.ComputeHash(a);

        string firstAA = _hashaMasha.ComputeHash(a);
        _hashaMasha.ComputeHash(b);
        string secondAA = _hashaMasha.ComputeHash(a);

        Assert.Equal(firstAI, secondAI);
        Assert.Equal(firstAA, secondAA);
    }

    [Fact]
    public void Utf8Input_ReturnsValidHash()
    {
        byte[] input = Encoding.UTF8.GetBytes("Žąsis 🙂");
        
        string hashI = _hashFunc.ComputeHash(input);
        string hashA = _hashaMasha.ComputeHash(input);
        AssertValidHash(hashI);
        AssertValidHash(hashA);
    }
    
    [Theory]
    [InlineData("abc", "abc\n")]
    [InlineData("abc", " abc")]
    [InlineData("abc", "abc ")]
    [InlineData("aaaa", "aaab")]
    [InlineData("AB", "BA")]
    public void SelectedDifferentInputsReturnDifferentHashes(string first, string second)
    {
        byte[] firstBytes = Encoding.UTF8.GetBytes(first);
        byte[] secondBytes = Encoding.UTF8.GetBytes(second);
        
        Assert.NotEqual(_hashFunc.ComputeHash(firstBytes), _hashFunc.ComputeHash(secondBytes));
        Assert.NotEqual(_hashaMasha.ComputeHash(firstBytes), _hashaMasha.ComputeHash(secondBytes));
    }

    [Fact]
    public void HashingDoesNotChangeInputBytes()
    {
        byte[] input = { 0x41, 0x00, 0x42 };
        byte[] original = input.ToArray();

        _hashFunc.ComputeHash(input);
        Assert.Equal(input, original);
        _hashaMasha.ComputeHash(input);
        Assert.Equal(original, input);
    }

    private static void AssertValidHash(string hash)
    {
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9A-F]{64}$", hash);
    }
}