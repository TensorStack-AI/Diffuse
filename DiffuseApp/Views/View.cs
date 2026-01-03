using System.Collections.Generic;

namespace Diffuse.Views
{
    public enum View
    {
        Home = 0,
        Settings = 1,

        TextToImage = 100,
        ImageToImage = 101,
        ImageEdit = 102,

        ControlNetImage = 110,
        ControlNetImageToImage = 111,

        TextToVideo = 200,
        ImageToVideo = 201,
        VideoToVideo = 202,
        History = 203
    }

    public enum ViewCategory
    {
        Other = 0,
        Image = 1,
        Video = 2
    }

    public static class ViewManager
    {

        private static readonly Dictionary<ViewCategory, View> CurrentViewMap = new Dictionary<ViewCategory, View>
        {
            {ViewCategory.Other, View.Settings },
          //  {ViewCategory.Text, View.TextSummary },
            {ViewCategory.Image, View.TextToImage },
            {ViewCategory.Video, View.TextToVideo },
       //    {ViewCategory.Audio, View.AudioTranscribe }
        };


        private static readonly Dictionary<View, ViewCategory> ViewCategoryMap = new Dictionary<View, ViewCategory>
        {
            // General
            { View.Settings, ViewCategory.Other  },

            // Image
            { View.TextToImage, ViewCategory.Image  },
            { View.ImageToImage, ViewCategory.Image  },
            { View.ImageEdit, ViewCategory.Image  },
            { View.ControlNetImage, ViewCategory.Image  },
            { View.ControlNetImageToImage, ViewCategory.Image  },

            // Video
            { View.TextToVideo, ViewCategory.Video  },
            { View.ImageToVideo, ViewCategory.Video  },
            { View.VideoToVideo, ViewCategory.Video  },
        };


        internal static View GetCurrentView(ViewCategory category)
        {
            return CurrentViewMap[category];
        }


        internal static ViewCategory SetCurrentView(View view)
        {
            var category = ViewCategoryMap[view];
            CurrentViewMap[category] = view;
            return category;
        }
    }
}
