using Della.Application.Service;
using Della.Application.Service.Interfaces;
using Della.Domain.Context;
using Della.Domain.Entities;
using Della.Domain.Repositories;
using Della.Domain.Repositories.Interfaces;
using SimpleInjector;

namespace Della.Services.IoC
{
    public class BootStrapper
    {
        public static void RegisterServices(Container container)
        {
            #region "Context"

            container.Register<DellaContext>(Lifestyle.Scoped);

            #endregion

            #region Inject Services

            container.Register<IUserService, UserService>(Lifestyle.Scoped);
            container.Register<ICountryService, CountryService>(Lifestyle.Scoped);
            container.Register<IStateService, StateService>(Lifestyle.Scoped);
            container.Register<ICityService, CityService>(Lifestyle.Scoped);
            container.Register<IPlaceService, PlaceService>(Lifestyle.Scoped);
            #endregion

            #region Inject Repositories

            container.Register<IRepository<User>, Repository<User>>(Lifestyle.Scoped);
            container.Register<IRepository<Country>, Repository<Country>>(Lifestyle.Scoped);
            container.Register<IRepository<State>, Repository<State>>(Lifestyle.Scoped);
            container.Register<IRepository<City>, Repository<City>>(Lifestyle.Scoped);
            container.Register<IRepository<Place>, Repository<Place>>(Lifestyle.Scoped);
            container.Register<IRepository<Check>, Repository<Check>>(Lifestyle.Scoped);

            #endregion

        }
    }
}
