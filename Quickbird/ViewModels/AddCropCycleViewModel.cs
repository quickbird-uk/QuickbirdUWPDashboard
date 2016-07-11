namespace Quickbird.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DbStructure;
    using DbStructure.User;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Util;

    public class AddCropCycleViewModel : ViewModelBase
    {
        private readonly List<KeyValuePair<string, CropType>> _cropTypeCache =
            new List<KeyValuePair<string, CropType>>();

        private readonly Action<string> _updateEvent;
        private bool _chosenIsVacant;
        private PlaceTuple _chosenPlace;


        private ObservableCollection<PlaceTuple> _placeList = new ObservableCollection<PlaceTuple>();
        private List<string> _suggestedList = new List<string>();
        private string _userEnteredCropType = string.Empty;


        public AddCropCycleViewModel()
        {
            _updateEvent = UpdateData;
            Messenger.Instance.TablesChanged.Subscribe(_updateEvent);
            UpdateData(string.Empty);
        }

        /// <summary>
        ///     List of places to be displayed to the user.
        /// </summary>
        public ObservableCollection<PlaceTuple> PlaceList
        {
            get { return _placeList; }
            private set
            {
                _placeList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     This is the selection of the user
        /// </summary>
        public object SelectedPlace
        {
            get { return _chosenPlace; }
            set
            {
                _chosenPlace = (PlaceTuple) value;
                ChosenIsVacant = _chosenPlace.IsVacant;
            }
        }

        /// <summary>
        ///     CropType Chosen By the user. Set By UI
        /// </summary>
        public string UserCropType
        {
            get { return _userEnteredCropType; }
            set
            {
                _userEnteredCropType = value;
                SuggestedList = _cropTypeCache.Where(ct => ct.Key
                        .Contains(_userEnteredCropType.ToLower()))
                    .Select(ct => ct.Value.Name).ToList();
            }
        }

        /// <summary>
        ///     Suggested Crop List for the suggestion box.
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
        ///     Indicates if the chosen Location is avaliable for a new cropRun to be created
        /// </summary>
        public bool ChosenIsVacant
        {
            get { return _chosenIsVacant; }
            set
            {
                _chosenIsVacant = value;
                OnPropertyChanged();
            }
        }

        public string CropVariety { get; set; }


        /// <summary>
        ///     Update Data event, runs every time
        /// </summary>
        /// <param name="something"></param>
        private async void UpdateData(string something)
        {
            using (var db = new MainDbContext())
            {
                var places =
                    await db.Locations.Where(loc => loc.Devices.Count > 0).Include(loc => loc.CropCycles).ToListAsync();

                var cropTypeCache = await db.CropTypes.ToListAsync();
                foreach (var cropType in cropTypeCache)
                {
                    if (_cropTypeCache.Any(ct => ct.Key.Equals(cropType.Name.ToLower())) == false)
                    {
                        var pair = new KeyValuePair<string, CropType>(cropType.Name.ToLower(),
                            cropType);
                        _cropTypeCache.Add(pair);
                    }
                }

                foreach (var loc in places)
                {
                    var tuple =
                        PlaceList.FirstOrDefault(ct => ct.Location.Equals(loc));
                    if (tuple == null)
                    {
                        tuple = new PlaceTuple {Location = loc};
                        PlaceList.Add(tuple);
                    }

                    var runningCropCycle = loc.CropCycles.FirstOrDefault(cc => cc.EndDate == null);
                    if (runningCropCycle != null)
                    {
                        tuple.DisplayName = loc.Name + " - already monitoring " + runningCropCycle.CropTypeName +
                                            " of variety " + runningCropCycle.CropVariety;
                        tuple.IsVacant = false;
                    }
                    else
                    {
                        tuple.DisplayName = loc.Name + " - Avaliable";
                        tuple.IsVacant = true;
                    }
                }
                OnPropertyChanged(nameof(PlaceList));
            }
        }

        public async void CreateNewCropRun()
        {
            ChosenIsVacant = false;
            var settings = Settings.Instance;
            var cropType = _cropTypeCache.FirstOrDefault(ct => ct.Key.Equals(UserCropType.ToLower())).Value
                           ?? new CropType
                           {
                               Name = UserCropType,
                               Approved = false,
                               CreatedAt = DateTimeOffset.Now,
                               CreatedBy = settings.CredStableSid
                           };
            var cropCycle = new CropCycle
            {
                ID = Guid.NewGuid(),
                Name = "Unnamed",
                Yield = 0,
                CropType = cropType,
                CropTypeName = cropType.Name,
                CropVariety = CropVariety,
                LocationID = _chosenPlace.Location.ID,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now,
                StartDate = DateTimeOffset.Now,
                EndDate = null,
                Deleted = false,
                Version = new byte[32]
            };

            using (var db = new MainDbContext())
            {
                db.CropCycles.Add(cropCycle);

                await db.SaveChangesAsync();
            }
            await Messenger.Instance.TablesChanged.Invoke(string.Empty);
        }


        public override void Kill()
        {
            Messenger.Instance.TablesChanged.Unsubscribe(_updateEvent);
        }


        /// <summary>
        ///     Used as a grouping for UI to blabber about
        /// </summary>
        public class PlaceTuple : ViewModelBase
        {
            private string _displayName;
            private bool _isVacant;
            private Location _location;

            //public override bool Equals(object obj)
            //{
            //    PlaceTuple other = obj as PlaceTuple; 
            //    return other?._displayName.ToLower().Equals(_displayName.ToLower()) ?? false;
            //}


            public string DisplayName
            {
                get { return _displayName; }
                set
                {
                    _displayName = value;
                    OnPropertyChanged();
                }
            }

            public Location Location
            {
                get { return _location; }
                set
                {
                    _location = value;
                    OnPropertyChanged();
                }
            }

            public bool IsVacant
            {
                get { return _isVacant; }
                set
                {
                    _isVacant = value;
                    OnPropertyChanged();
                }
            }

            public override void Kill()
            {
                // Nothing here a simple data class.
            }
        }
    }
}