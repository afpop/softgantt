using Della.Application.Service.Interfaces;
using Della.Application.ViewModel;
using Della.Application.ViewModel.User;
using Della.Domain.Entities;
using Della.Domain.Repositories.Interfaces;
using MultipartDataMediaFormatter.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Della.Application.Service
{
    public class PlaceService : IPlaceService
    {
        public IRepository<Place> PlaceRepository { get; }
        public IRepository<Photo> PhotoRepository { get; }
        public IRepository<Check> CheckRepository { get; }
        public IRepository<User> UserRepository { get; }

        public PlaceService(IRepository<Place> placeRepository, IRepository<Check> checkRepository,
            IRepository<User> userRepository)
        {
            PlaceRepository = placeRepository;
            CheckRepository = checkRepository;
            UserRepository = userRepository;
        }

        public RequestReturnVM<PlaceVM> Create(PlaceVM place)
        {
            try
            {
                Place _place = new Place();

                if (place.ID != 0)
                    _place = PlaceRepository.Find(wh => wh.ID == place.ID, i => i.Operation);

                _place.CityID = 2;
                _place.Name = place.Name;
                _place.Description = place.Description;
                _place.Address = place.Address;
                _place.Location = place.Location;
                _place.Verified = true;

                if (place.Operation != null && place.Operation.Count() > 0)
                {
                    if (_place.Operation == null)
                        _place.Operation = new List<Operation>();

                    var _operationsID = place.Operation.Select(sel => sel.ID).ToList();
                    var _operationToAdd = place.Operation.Where(wh => wh.ID == 0).ToList();
                    var _operationToRemove = _place.Operation.Where(wh => !_operationsID.Contains(wh.ID)).ToList();

                    _operationToAdd.ForEach(fe => { _place.Operation.Add(new Operation { Day = fe.Day, Start = fe.Start, End = fe.End }); });

                    _operationToRemove.ForEach(fe => { _place.Operation.Remove(fe); });
                }

                if (place.ID != 0)
                    PlaceRepository.Update(_place);
                else
                    PlaceRepository.Insert(_place);

                return new RequestReturnVM<PlaceVM>
                {
                    MessageBody = "Lugar cadastrado com sucesso!",
                    MessageTitle = "Sucesso",
                    Data = new PlaceVM(_place),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RequestReturnVM<List<PlaceVM>> GetAll()
        {
            try
            {
                var _places = PlaceRepository.FindAll(wh => wh.Active, i => i.Photos, i => i.Operation).ToList().Select(sel => new PlaceVM(sel)).ToList();

                _places.ForEach(fe => { fe.OperationStatus = GetOperationStatus(fe.Operation); });

                _places = _places.OrderByDescending(or => or.TotalEngaged).ToList();

                return new RequestReturnVM<List<PlaceVM>>
                {
                    MessageBody = "Sucesso ao obter lugares!",
                    MessageTitle = "Sucesso",
                    Data = _places,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RequestReturnVM<PlaceVM> Get(int placeID)
        {
            try
            {
                var _place = new PlaceVM(PlaceRepository.Find(wh => wh.ID == placeID, i => i.Photos, i => i.Operation));
                _place.OperationStatus = GetOperationStatus(_place.Operation);

                return new RequestReturnVM<PlaceVM>
                {
                    MessageBody = "Sucesso ao obter lugar!",
                    MessageTitle = "Sucesso",
                    Data = _place,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RequestReturnVM<bool> UploadPhoto(PhotoVM photo)
        {
            try
            {
                var _place = PlaceRepository.Find(wh => wh.ID == photo.PlaceID, i => i.Photos);

                Photo _photo = new Photo { Type = photo.Type, File = Convert.FromBase64String(photo.PhotoBase64) };

                _place.Photos.Add(_photo);

                PlaceRepository.Update(_place);

                return new RequestReturnVM<bool>
                {
                    MessageBody = "Sucesso ao carregar foto!",
                    MessageTitle = "Sucesso",
                    Data = true,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RequestReturnVM<bool> Interested(int placeID, IPrincipal user)
        {
            try
            {
                var _user = GetUser(user);

                var _place = PlaceRepository.Find(placeID);

                var _check = CheckRepository.Find(wh => wh.PlaceID == placeID && wh.UserID == _user.ID);

                if (_check != null)
                {
                    if (_check.Status == "I")
                    {
                        return new RequestReturnVM<bool>
                        {
                            MessageBody = "Sucesso ao marcar interesse.",
                            MessageTitle = "Sucesso",
                            Data = true,
                            Success = true
                        };
                    }
                    else
                    {
                        if (_check.Status == "G")
                            _place.Going--;
                        else
                            _place.OnSite--;

                        _check.Status = "I";

                        CheckRepository.Update(_check);
                    }
                }
                else
                {
                    Check _newCheck = new Check
                    {
                        PlaceID = _place.ID,
                        UserID = _user.ID,
                        Status = "I"
                    };

                    CheckRepository.Insert(_newCheck);
                }

                _place.Interesteds++;

                PlaceRepository.Update(_place);

                return new RequestReturnVM<bool>
                {
                    MessageBody = "Sucesso ao marcar interesse.",
                    MessageTitle = "Sucesso",
                    Data = true,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RequestReturnVM<bool> Going(int placeID, IPrincipal user)
        {
            try
            {
                var _user = GetUser(user);

                var _place = PlaceRepository.Find(placeID);

                var _check = CheckRepository.Find(wh => wh.PlaceID == placeID && wh.UserID == _user.ID);

                if (_check != null)
                {
                    if (_check.Status == "G")
                    {
                        return new RequestReturnVM<bool>
                        {
                            MessageBody = "Sucesso ao marcar presença.",
                            MessageTitle = "Sucesso",
                            Data = true,
                            Success = true
                        };
                    }
                    else
                    {
                        if (_check.Status == "I")
                            _place.Interesteds--;
                        else
                            _place.OnSite--;

                        _check.Status = "G";

                        CheckRepository.Update(_check);
                    }
                }
                else
                {
                    Check _newCheck = new Check
                    {
                        PlaceID = _place.ID,
                        UserID = _user.ID,
                        Status = "G"
                    };

                    CheckRepository.Insert(_newCheck);
                }

                _place.Going++;

                PlaceRepository.Update(_place);

                return new RequestReturnVM<bool>
                {
                    MessageBody = "Sucesso ao marcar presença.",
                    MessageTitle = "Sucesso",
                    Data = true,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RequestReturnVM<bool> CheckIn(int placeID, IPrincipal user)
        {
            try
            {
                var _user = GetUser(user);

                var _place = PlaceRepository.Find(placeID);

                var _check = CheckRepository.Find(wh => wh.PlaceID == placeID && wh.UserID == _user.ID);

                if (_check != null)
                {
                    if (_check.Status == "C")
                    {
                        return new RequestReturnVM<bool>
                        {
                            MessageBody = "Sucesso ao fazer check-in.",
                            MessageTitle = "Sucesso",
                            Data = true,
                            Success = true
                        };
                    }
                    else
                    {
                        if (_check.Status == "I")
                            _place.Interesteds--;
                        else
                            _place.Going--;

                        _check.Status = "C";

                        CheckRepository.Update(_check);
                    }
                }
                else
                {
                    Check _newCheck = new Check
                    {
                        PlaceID = _place.ID,
                        UserID = _user.ID,
                        Status = "C"
                    };

                    CheckRepository.Insert(_newCheck);
                }

                _place.OnSite++;

                PlaceRepository.Update(_place);

                return new RequestReturnVM<bool>
                {
                    MessageBody = "Sucesso ao fazer check-in.",
                    MessageTitle = "Sucesso",
                    Data = true,
                    Success = true
                }; ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RequestReturnVM<List<AvatarVM>> GetAvatarByStatus(int placeID, string status)
        {
            try
            {
                var _users = CheckRepository.FindAll(wh => wh.PlaceID == placeID && wh.Status == status, i => i.User).Take(15).ToList().Select(sel => new AvatarVM(sel.User)).ToList();

                return new RequestReturnVM<List<AvatarVM>>
                {
                    MessageBody = "Sucesso ao obter avatares!",
                    MessageTitle = "Sucesso",
                    Data = _users,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static UserVM GetUser(IPrincipal principal)
        {
            try
            {
                // Carrega as Claim para pegar o UserName & Role
                var identity = (ClaimsIdentity)principal.Identity;
                IEnumerable<Claim> claims = identity.Claims;

                var userIdObj = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var usuarioObj = claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault();
                var perfisObj = claims.Where(x => x.Type == ClaimTypes.Role).FirstOrDefault();

                if (userIdObj == null) throw new Exception("Claim NameIdentifier não encontrada!");
                if (usuarioObj == null) throw new Exception("Claim UserName não encontrada!");
                if (perfisObj == null) throw new Exception("Claim Role não encontrada!");

                var userId = userIdObj.Value;
                var usuario = usuarioObj.Value;
                var listaPerfils = perfisObj.Value.Split(';');
                List<int> listaPerfis = new List<int>();
                //listaPerfils.ToList().ForEach(x => listaPerfis.Add(Convert.ToInt32(x)));


                if (userId == string.Empty) throw new Exception("Claim NameIdentifier não possuia valor associado!");
                if (usuario == string.Empty) throw new Exception("Claim UserName não possuia valor associado!");
                if (listaPerfils == null || listaPerfils.Count() < 1) throw new Exception("Claim Roles não possuia valor associado!");

                var user = new UserVM()
                {
                    ID = userId
                };

                return user;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private OperationStatusVM GetOperationStatus(List<OperationVM> operation)
        {
            if (operation.Count() <= 0)
                return new OperationStatusVM { Value = 0, Text = "Indisponível" };

            var _dateNow = DateTime.Now;
            var _dayNow = (int)_dateNow.DayOfWeek;
            var _tomorrow = (int)_dateNow.AddDays(1).DayOfWeek;
            var _timeNow = _dateNow.TimeOfDay;

            var _operationsOfDay = operation.Where(wh => wh.Day == _dayNow).ToList();

            if (_operationsOfDay.Count() <= 0)
            {
                if (operation.Where(wh => wh.Day == _tomorrow).Count() > 0)
                    return new OperationStatusVM { Value = 1, Text = "Abre amanhã" };
                else
                    return new OperationStatusVM { Value = 2, Text = "Fechado" };
            }

            foreach (var _operation in _operationsOfDay)
            {
                var _startDate = DateTime.Now.Date + _operation.Start;
                var _endDate = _operation.End > new TimeSpan(0, 0, 0) && _operation.End < new TimeSpan(8, 0, 0) ? DateTime.Now.AddDays(1) : DateTime.Now;
                _endDate = _endDate.Date + _operation.End;

                if (_dateNow > _startDate && _dateNow < _endDate)
                    return new OperationStatusVM { Value = 3, Text = "Aberto" };
            }

            if (_operationsOfDay.Where(wh => wh.Start > _timeNow).Count() > 0)
                return new OperationStatusVM { Value = 4, Text = "Abre hoje" };

            return new OperationStatusVM { Value = 0, Text = "Indisponível" };
        }

        public void Robots()
        {
            var _places = PlaceRepository.FindAll(wh => wh.Active, i => i.Operation).ToList();

            foreach (var _place in _places)
            {
                Random random = new Random();

                var _status = GetOperationStatus(_place.Operation.Select(sel => new OperationVM(sel)).ToList()).Text;

                if (_status == "Abre hoje" || _status == "Aberto")
                {
                    _place.Interesteds = random.Next(5, 50);
                    _place.Going = random.Next(5, 35);
                }

                if (_status == "Aberto")
                    _place.OnSite = random.Next(5, 20);

                for (int i = 0; i < _place.Interesteds; i++)
                {
                    Check _check = new Check
                    {
                        PlaceID = _place.ID,
                        UserID = "4299f0df-32e8-442e-83fc-4a2149b11aa5",
                        Status = "I"
                    };

                    CheckRepository.Insert(_check);
                }

                for (int i = 0; i < _place.Going; i++)
                {
                    Check _check = new Check
                    {
                        PlaceID = _place.ID,
                        UserID = "4299f0df-32e8-442e-83fc-4a2149b11aa5",
                        Status = "G"
                    };

                    CheckRepository.Insert(_check);
                }

                for (int i = 0; i < _place.OnSite; i++)
                {
                    Check _check = new Check
                    {
                        PlaceID = _place.ID,
                        UserID = "4299f0df-32e8-442e-83fc-4a2149b11aa5",
                        Status = "C"
                    };

                    CheckRepository.Insert(_check);
                }

                PlaceRepository.Update(_place);
            }
        }
    }
}
