namespace OpenIMSBackend.Core.Services
{
    /// <summary>
    /// Creates the data which a freshly started backend needs so that it is usable immediately without setting
    /// anything up by hand. It is only used in the environment <see cref="GRYLibrary.Core.APIServer.ConcreteEnvironments.Development"/>.
    /// <para>
    /// The example-data consists of user-accounts only. As soon as OpenIMS has business-content, the creation of
    /// that content belongs here as well.
    /// </para>
    /// </summary>
    public class ExampleDataCreator : IExampleDataCreator
    {
        /// <summary>The amount of example-users which are created. Their names are "user01", "user02", ...</summary>
        private const int AmountOfExampleUsers = 2;

        private readonly IBusinessLogicService _BusinessLogicService;

        /// <summary>Initializes a new instance of <see cref="ExampleDataCreator"/>.</summary>
        /// <param name="businessLogicService">The business logic service used to create the example-entities.</param>
        public ExampleDataCreator(IBusinessLogicService businessLogicService)
        {
            this._BusinessLogicService = businessLogicService;
        }

        /// <inheritdoc />
        public void AddExampleData()
        {
            for (int userNumber = 1; userNumber <= AmountOfExampleUsers; userNumber++)
            {
                this.AddUser(userNumber);
            }
        }

        /// <summary>Creates one example-user whose password is equal to its name, so that it can be used to log in without looking anything up.</summary>
        /// <param name="userNumber">The number of the user, which becomes a part of its name.</param>
        /// <returns>The id of the created user.</returns>
        private string AddUser(int userNumber)
        {
            string userName = $"user{userNumber.ToString().PadLeft(2, '0')}";
            string password = userName;
            return this._BusinessLogicService.Register(userName, password);
        }
    }
}
