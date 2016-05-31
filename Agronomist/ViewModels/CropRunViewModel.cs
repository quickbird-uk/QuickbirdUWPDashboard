namespace Agronomist.ViewModels
{
    using System;
    using DatabasePOCOs.User;

    internal class CropRunViewModel : ViewModelBase
    {
        private Guid _cropRunId;

        

        public CropRunViewModel(CropCycle cropRun)
        {
            _cropRunId = cropRun.ID;
            Update(cropRun);
        }

        public void Update(CropCycle cropRun)
        {
            
        }
    }
}