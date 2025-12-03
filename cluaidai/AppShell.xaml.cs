using cluaidai.Views;

namespace cluaidai
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("conversation", typeof(ConversationDetailPage));
            Routing.RegisterRoute("search", typeof(SearchPage));
            Routing.RegisterRoute("createpost", typeof(CreatePostPage));
        }
    }
}
