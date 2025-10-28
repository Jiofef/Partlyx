using Avalonia.Markup.Xaml.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//namespace Partlyx.UI.Avalonia.DataTemplateSelectors
//{
//    public class IconContentTemplateSelector : DataTemplateSelector
//    {
//        public DataTemplate ImageSourceTemplate { get; set; }
//        public DataTemplate DrawingTemplate { get; set; }
//        public DataTemplate EnumTemplate { get; set; }
//        public DataTemplate GlyphTemplate { get; set; }
//        public DataTemplate FallbackTemplate { get; set; }

//        public override DataTemplate SelectTemplate(object item, DependencyObject container)
//        {
//            if (item == null) return FallbackTemplate;

//            // ImageSource (BitmapImage, DrawingImage.Source, etc.)
//            if (item is ImageSource) return ImageSourceTemplate;

//            // DrawingGroup/Geometry
//            if (item is DrawingGroup || item is Drawing) return DrawingTemplate;

//            // boxed enum (FontAwesome enum or similar)
//            var t = item.GetType();
//            if (t.IsEnum) return EnumTemplate;

//            // fallback
//            return FallbackTemplate;
//        }
//    }
//}