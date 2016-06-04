using DatabasePOCOs.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Agronomist.Models;
using DatabasePOCOs;
using Agronomist.Util;

namespace Agronomist.ViewModels
{
    public class AddCropCycleViewModel : ViewModelBase
    {
        MainDbContext _db = new MainDbContext();

        private ObservableCollection<PlaceTuple> _placeList = new ObservableCollection<PlaceTuple>();
        private PlaceTuple _chosenPlace = null;
        private bool _chosenIsVacant = false;
        private string _userEnteredCropType = string.Empty;
        private List<string> _suggestedList = new List<string>(); 
        private List<KeyValuePair<string, CropType>> _CropTypeCache = new List<KeyValuePair<string, CropType>>(); 
        private Action<string> updateEvent; 

        

        public AddCropCycleViewModel()
        {
            updateEvent = UpdateData;
            Messenger.Instance.TablesChanged.Subscribe(updateEvent); 
            UpdateData(string.Empty); 
        }


        /// <summary>
        /// Update Data event, runs every time 
        /// </summary>
        /// <param name="something"></param>
        private async void UpdateData(string something)
        {
            var places = await _db.Locations.Where(loc => loc.Devices.Count > 0).Include(loc => loc.CropCycles).ToListAsync();
            var cropTypeCache = await _db.CropTypes.ToListAsync();
            foreach (CropType cropType in cropTypeCache)
            {
                if (_CropTypeCache.Any(ct => ct.Key.Equals(cropType.Name.ToLower())) == false)
                {
                    var pair = new KeyValuePair<string, CropType>(cropType.Name.ToLower(),
                        cropType);
                    _CropTypeCache.Add(pair);
                }
            }

            foreach (Location loc in places)
            {
                PlaceTuple tuple =
                    PlaceList.FirstOrDefault(ct => ct.Location.Equals(loc));
                if (tuple == null)
                {
                    tuple = new PlaceTuple { Location = loc };
                    PlaceList.Add(tuple);
                }

                CropCycle runningCropCycle = loc.CropCycles.FirstOrDefault(cc => cc.EndDate == null);
                if (runningCropCycle != null)
                {
                    tuple.DisplayName = loc.Name + " - already monitoring " + runningCropCycle.CropTypeName + " of variety " + runningCropCycle.CropVariety;
                    tuple.IsVacant = false;
                }
                else
                {
                    tuple.DisplayName = loc.Name + " - Avaliable";
                    tuple.IsVacant = true;
                }

               
            }
            OnPropertyChanged("PlaceList");
        }

        /// <summary>
        /// List of places to be displayed to the user. 
        /// </summary>
        public ObservableCollection<PlaceTuple> PlaceList
        { get {
                return _placeList; }
            private set {
                _placeList = value;
                OnPropertyChanged(); 
            }
        }

        /// <summary>
        /// This is the selection of the user 
        /// </summary>
        public Object SelectedPlace {
            get { return _chosenPlace; }
            set
            {
                _chosenPlace = (PlaceTuple)value;
                ChosenIsVacant = _chosenPlace.IsVacant; 
            }
        }

        /// <summary>
        /// CropType Chosen By the user. Set By UI
        /// </summary>
        public string UserCropType
        {
            get { return _userEnteredCropType; }
            set
            {
                _userEnteredCropType = value;                
                SuggestedList = _CropTypeCache.Where(ct => ct.Key
                .Contains(_userEnteredCropType.ToLower()))
                .Select(Ct => Ct.Value.Name).ToList(); 
            }
        }

        /// <summary>
        /// Suggested Crop List for the suggestion box. 
        /// </summary>
        public List<string> SuggestedList
        {
            get { return _suggestedList; }
            set
            {
                _suggestedList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates if the chosen Location is avaliable for a new cropRun to be created
        /// </summary>
        public Boolean ChosenIsVacant
        {
            get { return _chosenIsVacant; }
            set {
                _chosenIsVacant = value;
                OnPropertyChanged();                 
            }
        }

        public string CropVariety { get; set; }

        public async void CreateNewCropRun()
        {
            ChosenIsVacant = false; 
            Settings settings = new Settings();
            CropType cropType = _CropTypeCache.FirstOrDefault(ct => ct.Key.Equals(UserCropType.ToLower())).Value
                ?? new CropType
                {
                    Name = UserCropType,
                    Approved = false,
                    CreatedAt = DateTimeOffset.Now,
                    CreatedBy = settings.CredStableSid
                };
            CropCycle cropCycle = new CropCycle
            {
                ID = Guid.NewGuid(),
                Name = "Unnamed",
                Yield = 0,
                CropType = cropType,
                CropTypeName = cropType.Name,
                CropVariety = CropVariety,
                LocationID = _chosenPlace.Location.ID,
                Location = _chosenPlace.Location,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now,
                StartDate = DateTimeOffset.Now,
                EndDate = null,
                Deleted = false,
                Version = new byte[32]
            };
            _db.CropCycles.Add(cropCycle);

            await _db.SaveChangesAsync();
            await Messenger.Instance.TablesChanged.Invoke(string.Empty); 
        }




        /// <summary>
        /// Used as a grouping for UI to blabber about
        /// </summary>
        public class PlaceTuple : ViewModelBase
        {
            private string _displayName;
            private Location _location;
            private bool _isVacant;

            //public override bool Equals(object obj)
            //{
            //    PlaceTuple other = obj as PlaceTuple; 
            //    return other?._displayName.ToLower().Equals(_displayName.ToLower()) ?? false;
            //}


            public string DisplayName {
                get { return _displayName; }
                set {_displayName = value;
                    OnPropertyChanged(); 
                }
            }
            public Location Location {
                get { return _location; }
                set
                {
                    _location = value;
                    OnPropertyChanged(); 
                }
            }
            public Boolean IsVacant {
                get { return _isVacant; }
                set
                {
                    _isVacant = value;
                    OnPropertyChanged();
                }
            }
        }



    }

    
}
