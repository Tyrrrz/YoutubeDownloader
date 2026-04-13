using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace YoutubeDownloader.Utils.Extensions;

internal static class NotifyPropertyChangedExtensions
{
    extension<TOwner>(TOwner owner)
        where TOwner : INotifyPropertyChanged
    {
        public IDisposable WatchProperty<TProperty>(
            Expression<Func<TOwner, TProperty>> propertyExpression,
            Action<TProperty> callback,
            bool watchInitialValue = false
        )
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression?.Member is not PropertyInfo property)
                throw new ArgumentException("Provided expression must reference a property.");

            var getValue = propertyExpression.Compile();

            void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
            {
                if (
                    string.IsNullOrWhiteSpace(args.PropertyName)
                    || string.Equals(args.PropertyName, property.Name, StringComparison.Ordinal)
                )
                {
                    callback(getValue(owner));
                }
            }

            owner.PropertyChanged += OnPropertyChanged;

            if (watchInitialValue)
                callback(getValue(owner));

            return Disposable.Create(() => owner.PropertyChanged -= OnPropertyChanged);
        }

        public IDisposable WatchAllProperties(Action callback, bool watchInitialValues = false)
        {
            void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) => callback();
            owner.PropertyChanged += OnPropertyChanged;

            if (watchInitialValues)
                callback();

            return Disposable.Create(() => owner.PropertyChanged -= OnPropertyChanged);
        }
    }
}
