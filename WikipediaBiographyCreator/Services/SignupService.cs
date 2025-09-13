using WikipediaBiographyCreator.Exceptions;
using WikipediaBiographyCreator.Interfaces;
using WikipediaBiographyCreator.Models;

public class SignupService : ISignupService
{
    public List<Signup> Get()
    {
        return new List<Signup>
        {
            new Signup { Id = "1", Name = "Alice", PhoneNumber = "123-456-7890", PartySize = 4 },
            new Signup { Id = "2", Name = "Bob", PhoneNumber = "987-654-3210", PartySize = 2 }
        };
    }

    public void TestStuff()
    {
        throw new AppException("Scaffolding complete...");
    }
}
