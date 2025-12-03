using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using cluaidai.Services;
using cluaidai.ViewModels;
using cluaidai.Views;

namespace cluaidai
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register Services
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<SignalRService>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<ApiService>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<FeedViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ConversationDetailViewModel>();
            builder.Services.AddTransient<SearchViewModel>();
            builder.Services.AddTransient<CreatePostViewModel>();

            // Register Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MainFeedPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ConversationDetailPage>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<CreatePostPage>();

            return builder.Build();
        }
    }
}
