﻿using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Glimpse.Core;
using Glimpse.Core.Extensibility;

namespace Glimpse.Mvc3.Plumbing
{
    internal class GlimpseViewEngine : IViewEngine
    {
        public IViewEngine ViewEngine { get; set; }

        public GlimpseViewEngine(IViewEngine viewEngine)
        {
            ViewEngine = viewEngine;
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName,
                                                bool useCache)
        {
            ViewEngineResult viewEngineResult;

            using (GlimpseTimer.Start("Find Partial View: " + partialViewName, "View", string.Format("{0}{1}", ViewEngine.GetType().Name, useCache ? " cache" : "")))
            {
                viewEngineResult = ViewEngine.FindPartialView(controllerContext, partialViewName, useCache);
            }

            viewEngineResult = Process(viewEngineResult, true, partialViewName, "", useCache);

            return viewEngineResult;
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName,
                                         bool useCache)
        {
            ViewEngineResult viewEngineResult;

            using (GlimpseTimer.Start("Find View: " + viewName, "View", string.Format("{0}{1}", ViewEngine.GetType().Name, useCache ? " cache":"")))
            {
                viewEngineResult = ViewEngine.FindView(controllerContext, viewName, masterName, useCache);
            }

            viewEngineResult = Process(viewEngineResult, false, viewName, masterName, useCache);

            return viewEngineResult;
        }

        private ViewEngineResult Process(ViewEngineResult viewEngineResult, bool isPartial, string viewName,
                                                string masterName, bool useCache)
        {
            var contextStore = HttpContext.Current.Items;//Can this be removed?
            var vmStore = contextStore[GlimpseConstants.ViewEngine] as IList<GlimpseViewEngineCallMetadata>;

            if (vmStore == null)
                contextStore[GlimpseConstants.ViewEngine] = vmStore = new List<GlimpseViewEngineCallMetadata>();

            GlimpseView glimpseView = null;

            if (viewEngineResult.View != null)
            {
                //wrap up IView so we can get access to ViewContext
                glimpseView = new GlimpseView(viewEngineResult.View);
                glimpseView.ViewName = viewName;
                viewEngineResult = new ViewEngineResult(glimpseView, viewEngineResult.ViewEngine);
            }

            var metadata = new GlimpseViewEngineCallMetadata
                               {
                                   ViewEngineResult = viewEngineResult,
                                   IsPartial = isPartial,
                                   ViewName = viewName,
                                   MasterName = masterName,
                                   UseCache = useCache,
                                   GlimpseView = glimpseView,
                                   ViewEngineName = ViewEngine.GetType().Name
                               };
            vmStore.Add(metadata);

            return viewEngineResult;
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            ViewEngine.ReleaseView(controllerContext, view);
        }
    }
}