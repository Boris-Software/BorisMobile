using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BorisMobile.Controls.ViewModels
{
    public class ImagePickerViewModel : BindableObject
    {
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public ICommand PickImageCommand { get; }

        public ImagePickerViewModel()
        {
            PickImageCommand = new Command(async () => await PickImageAsync());
        }

        private async Task PickImageAsync()
        {
            try
            {
                var file = await MediaPicker.Default.CapturePhotoAsync();
                if (file != null)
                {
                    ImageSource = ImageSource.FromFile(file.FullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error picking image: {ex.Message}");
            }
        }
    }
}
