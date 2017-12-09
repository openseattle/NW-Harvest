using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace NWHarvest.Web.Helper
{
    public static class MvcHtmlHelper
    {
        // reference https://stackoverflow.com/questions/29808573/getting-the-values-from-a-nested-complex-object-that-is-passed-to-a-partial-view
        public static MvcHtmlString PartialFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            object model = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).Model;
            var viewData = new ViewDataDictionary(helper.ViewData)
            {
                TemplateInfo = new System.Web.Mvc.TemplateInfo { HtmlFieldPrefix = name }
            };
            return helper.Partial(partialViewName, model, viewData);
        }
    }
}