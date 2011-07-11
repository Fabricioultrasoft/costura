
public class ClassToTest
{
	public string Foo()
	{
		return ClassToReference.Foo();
	}
	public void ThrowException()
	{
		ClassToReference.ThrowException();
	}
}

