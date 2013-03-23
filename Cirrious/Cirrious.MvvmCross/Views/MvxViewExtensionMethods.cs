// MvxViewExtensionMethods.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Linq;
using Cirrious.CrossCore.Exceptions;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;

namespace Cirrious.MvvmCross.Views
{
    public static class MvxViewExtensionMethods
    {
        public static void OnViewCreate(this IMvxView view, Func<IMvxViewModel> viewModelLoader)
        {
            // note - we check the DataContent before the ViewModel to avoid casting errors
            //       in the case of 'simple' binding code
            if (view.DataContext != null)
                return;

            if (view.ViewModel != null)
                return;

            var viewModel = viewModelLoader();
            if (viewModel == null)
            {
                MvxTrace.Warning( "ViewModel not loaded for view {0}", view.GetType().Name);
                return;
            }

            view.ViewModel = viewModel;
        }


        public static void OnViewNewIntent(this IMvxView view, Func<IMvxViewModel> viewModelLoader)
        {
            MvxTrace.Warning(
                           "OnViewNewIntent isn't well understood or tested inside MvvmCross - it's not really a cross-platform concept.");
            throw new MvxException("OnViewNewIntent is not implemented");
        }

        public static void OnViewDestroy(this IMvxView view)
        {
            // nothing needed currently
        }

        public static Type ReflectionGetViewModelType(this IMvxView view)
        {
            if (view == null)
                return null;

            var propertyInfo = view.GetType().GetProperty("ViewModel");

            if (propertyInfo == null)
                return null;

            return propertyInfo.PropertyType;
        }

        public static IMvxViewModel ReflectionGetViewModel(this IMvxView view)
        {
            if (view == null)
                return null;

            var propertyInfo = view.GetType().GetProperty("ViewModel");

            if (propertyInfo == null)
                return null;

            return (IMvxViewModel) propertyInfo.GetGetMethod().Invoke(view, new object[] {});
        }

        public static IMvxBundle CreateSaveStateBundle(this IMvxView view)
        {
            var toReturn = new MvxBundle();

            var viewModel = view.ViewModel;
            if (viewModel == null)
                return toReturn;

            var methods = viewModel.GetType()
                            .GetMethods()
                            .Where(m => m.Name == "SaveState")
                            .Where(m => m.ReturnType != typeof(void))
                            .Where(m => !m.GetParameters().Any());

            foreach (var methodInfo in methods)
            {
                // use methods like `public T SaveState()`
                var stateObject = methodInfo.Invoke(viewModel, new object[0]);
                if (stateObject != null)
                {
                    toReturn.Write(stateObject);
                }
            }

            // call the general `public void SaveState(bundle)` method too
            viewModel.SaveState(toReturn);

            return toReturn;
        }
    }
}