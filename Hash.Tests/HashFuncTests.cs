using System.Text;
using Hash.Hashing;

namespace Hash.Tests;

public class HashFuncTests
{
    private HashFunc _hashFunc = new();

    [Fact]
    public void EmptyInputReturnsValidHash()
    {
        string hash = _hashFunc.ComputeHash(Array.Empty<byte>());
        AssertValidHash(hash);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(65)]
    [InlineData(255)]
    public void SingleByteInputReturnsValidHash(int value)
    {
        string hash = _hashFunc.ComputeHash(new[] { (byte)value });
        AssertValidHash(hash);
    }

    [Fact]
    public void SameInputReturnsSameHash()
    {
        byte[] input = Encoding.UTF8.GetBytes("Hello World");

        string first = _hashFunc.ComputeHash(input);
        string second = _hashFunc.ComputeHash(input);

        Assert.Equal(first, second);
    }

    [Fact]
    public void SequenceABAReturnsSameHashForBothAValues()
    {
        byte[] a = Encoding.UTF8.GetBytes("A");
        byte[] b = Encoding.UTF8.GetBytes("B");

        string firstA = _hashFunc.ComputeHash(a);
        _hashFunc.ComputeHash(b);
        string secondA = _hashFunc.ComputeHash(a);

        Assert.Equal(firstA, secondA);
    }

    [Fact]
    public void Utf8Input_ReturnsValidHash()
    {
        byte[] input = Encoding.UTF8.GetBytes("Žąsis 🙂");

        string hash = _hashFunc.ComputeHash(input);

        AssertValidHash(hash);
    }

    [Fact]
    public void SelectedDifferentInputs_ReturnDifferentHashes()
    {
        string first = _hashFunc.ComputeHash(Encoding.UTF8.GetBytes("AB"));
        string second = _hashFunc.ComputeHash(Encoding.UTF8.GetBytes("BA"));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Hashing_DoesNotChangeInputBytes()
    {
        byte[] input = { 0x41, 0x00, 0x42 };
        byte[] original = input.ToArray();

        _hashFunc.ComputeHash(input);

        Assert.Equal(original, input);
    }

    [Fact]
    public void NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _hashFunc.ComputeHash(null!));
    }

    private static void AssertValidHash(string hash)
    {
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9A-F]{64}$", hash);
    }
}