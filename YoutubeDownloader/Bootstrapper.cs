using Stylet;
using StyletIoC;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);

            builder.Bind<IViewModelFactory>().ToAbstractFactory();
        }
    }
}