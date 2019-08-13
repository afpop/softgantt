using Della.Application.ViewModel;
using MultipartDataMediaFormatter.Infrastructure;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;

namespace Della.Application.Service.Interfaces
{
    public interface IPlaceService
    {
        RequestReturnVM<PlaceVM> Create(PlaceVM city);

        RequestReturnVM<PlaceVM> Get(int placeID);

        RequestReturnVM<List<PlaceVM>> GetAll();

        RequestReturnVM<bool> UploadPhoto(PhotoVM photo);

        RequestReturnVM<bool> Interested(int placeID, IPrincipal user);

        RequestReturnVM<bool> Going(int placeID, IPrincipal user);

        RequestReturnVM<bool> CheckIn(int placeID, IPrincipal user);

        RequestReturnVM<List<AvatarVM>> GetAvatarByStatus(int placeID, string status);

        void Robots();
    }
}
