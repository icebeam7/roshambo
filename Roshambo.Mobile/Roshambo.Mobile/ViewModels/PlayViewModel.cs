using System;
using System.Windows.Input;
using System.Threading.Tasks;

using Xamarin.Forms;

using Roshambo.Mobile.Models;
using Roshambo.Mobile.Services;

namespace Roshambo.Mobile.ViewModels
{
    public class PlayViewModel : BaseViewModel
    {
        private LocalPhoto localPhoto;

        public LocalPhoto LocalPhoto
        {
            get => localPhoto;
            set { SetProperty(ref localPhoto, value); OnPropertyChanged("PhotoFullPath"); }
        }

        public string PhotoFullPath
        {
            get 
            { 
                var path = LocalPhoto?.PhotoFile?.FullPath; 
                return path; 
            }
        }

        private string playerMove;

        public string PlayerMove
        {
            get => playerMove;
            set { SetProperty(ref playerMove, value); }
        }

        private string cpuMove;

        public string CpuMove
        {
            get => cpuMove;
            set { SetProperty(ref cpuMove, value); }
        }

        private string playResult;

        public string PlayResult
        {
            get => playResult;
            set { SetProperty(ref playResult, value); }
        }

        public ICommand ClassifyCommand { private set; get; }
        public ICommand TakePhotoCommand { private set; get; }

        private Random randomizer;

        private async Task takePhoto(bool mode)
        {
            PlayResult = string.Empty;
            LocalPhoto = await CameraService.TakePhoto(mode);
        }

        private async Task classify()
        {
            IsBusy = true;

            var mlGesture = await ClassificationService.Classify(LocalPhoto.PhotoFile);
            //await App.Current.MainPage.DisplayAlert("Result", mlGesture.PredictedLabel, "OK");

            PlayerMove = mlGesture.PredictedLabel;
            CpuMove = getCpuMove();
            PlayResult = evaluatePlay(PlayerMove, CpuMove);

            IsBusy = false;
        }

        private string getCpuMove()
        {
            var play = randomizer.Next(0, 3);

            switch(play)
            {
                case 0: return "rock";
                case 1: return "paper";
                case 2: return "scissors";
            }

            return "N/A";
        }

        private string evaluatePlay(string player, string cpu)
        {
            if (player == cpu)
                return "TIE";

            if (player == "rock")
                return (cpu == "scissors") ? "PLAYER WINS" : "CPU WINS";

            if (player == "paper")
                return (cpu == "rock") ? "PLAYER WINS" : "CPU WINS";

            if (player == "scissors")
                return (cpu == "paper") ? "PLAYER WINS" : "CPU WINS";

            return "N/A";
        }

        public PlayViewModel()
        {
            LocalPhoto = new LocalPhoto();
            PlayResult = string.Empty;
            randomizer = new Random();

            TakePhotoCommand = new Command<bool>(async (mode) => await takePhoto(mode));
            ClassifyCommand = new Command(async () => await classify());
        }
    }
}