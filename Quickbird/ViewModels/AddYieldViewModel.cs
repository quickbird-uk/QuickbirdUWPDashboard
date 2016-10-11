namespace Quickbird.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;
    using Qb.Poco.User;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Util;

    public class AddYieldViewModel : ViewModelBase
    {
        private readonly Guid _cropCycleId;
        private string _buttonText = "Add Yield";
        private bool _closeCropRun;
        private Visibility _errorVisibility = Visibility.Collapsed;
        private bool _isLoading;

        private SolidColorBrush _textBoxColour = new SolidColorBrush {Color = Colors.LightCyan};

        private Action<string> _updateAction;
        private double _userEnteredAmount;
        private bool _validEntry = true;


        public AddYieldViewModel(Guid cropCycleId)
        {
            _cropCycleId = cropCycleId;
            _updateAction = UpdateData;
            Messenger.Instance.TablesChanged.Subscribe(_updateAction);
            UpdateData(string.Empty);
        }

        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Updated from UI through two-way binding, determines if the crop run will be closed</summary>
        public bool CloseCropRun
        {
            get { return _closeCropRun; }
            set
            {
                _closeCropRun = value;
                ButtonText = value ? "End Crop" : "Add Yield";
            }
        }

        public Visibility ErrorVisibility
        {
            get { return _errorVisibility; }
            set
            {
                _errorVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Goes red when user input is invalid</summary>
        public SolidColorBrush TextBoxColour
        {
            get { return _textBoxColour; }
            set
            {
                _textBoxColour = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Updated from UI throught Two-wauy binding, processes user input</summary>
        public string UserEnteredText
        {
            get { return string.Empty; }
            set
            {
                var success = double.TryParse(value, out _userEnteredAmount);
                if (string.IsNullOrWhiteSpace(value))
                {
                    success = true;
                    _userEnteredAmount = 0;
                }

                //Only take action if something has changed!
                if (success != ValidEntry && success)
                {
                    ValidEntry = success;
                    ErrorVisibility = Visibility.Collapsed;
                    TextBoxColour = new SolidColorBrush {Color = Colors.LightCyan};
                }
                else if (success != ValidEntry && success == false)
                {
                    ValidEntry = success;
                    ErrorVisibility = Visibility.Visible;
                    TextBoxColour = new SolidColorBrush {Color = Colors.PaleVioletRed};
                }
            }
        }

        /// <summary>Goes true when user entered valid input, and lets hium proceed</summary>
        public bool ValidEntry
        {
            get { return _validEntry; }
            set
            {
                _validEntry = value;
                OnPropertyChanged();
            }
        }

        public override void Kill() { Messenger.Instance.TablesChanged.Unsubscribe(_updateAction); }

        /// <summary>Runs when the user licks the button</summary>
        public async Task SaveCropRun()
        {
            using (var db = new MainDbContext())
            {
                var cropCycle = await db.CropCycles.FirstAsync(cc => cc.Id == _cropCycleId);
                ValidEntry = false;
                IsLoading = true;
                cropCycle.Yield += _userEnteredAmount;
                if (_closeCropRun)
                    cropCycle.EndDate = DateTimeOffset.Now;
                cropCycle.UpdatedAt = DateTimeOffset.Now;
                await db.SaveChangesAsync();
                _updateAction = null;
                await Messenger.Instance.TablesChanged.Invoke(string.Empty);
                IsLoading = false;
            }
        }

        /// <summary>Attached to tables changed event</summary>
        /// <param name="tablesChangedPlaceholderVar">ignored</param>
        private async void UpdateData(string tablesChangedPlaceholderVar)
        {
            CropCycle cropCycle;
            using (var db = new MainDbContext())
            {
                cropCycle = await db.CropCycles.FirstAsync(cc => cc.Id == _cropCycleId);
            }
            if (cropCycle.EndDate != null)
            {
                //TODO Close this frame bacuse the crop cycle is already closed!
            }
        }
    }
}
