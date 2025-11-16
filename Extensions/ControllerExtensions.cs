using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;
using System;

namespace WebBanPhanMem.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync(this Controller controller, string viewName, object? model = null)

        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;

            controller.ViewData.Model = model;

            using var writer = new StringWriter();
            var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

            if (viewEngine == null)
                throw new InvalidOperationException("Không thể lấy ICompositeViewEngine từ dịch vụ.");

            var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

            if (!viewResult.Success)
            {
                throw new FileNotFoundException($"View '{viewName}' không tìm thấy.");
            }

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return writer.ToString();
        }
    }
}
