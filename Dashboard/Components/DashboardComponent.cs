﻿using Dashboard.Config;
using Dashboard.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dashboard.Components
{
    [ContainsConfig]
    public abstract class DashboardComponent : NotifyPropertyChanged
    {
        private ComponentPosition position = new ComponentPosition();
        [PersistentConfig]
        public ComponentPosition Position { get => position; set => SetAndNotify(ref position, value); }

        private bool showTitle = true;
        [PersistentConfig]
        public bool ShowTitle { get => showTitle; set => SetAndNotify(ref showTitle, value); }

        public abstract string DefaultName { get; }

        private string customName = "";
        [PersistentConfig]
        public string CustomName { get => customName; set => SetAndNotify(ref customName, value, new[] { nameof(Name) }); }

        public virtual string Name { get => CustomName.IsNullOrEmpty() ? DefaultName : CustomName; }

        public virtual void Initialize()
        {
            OnInitialize();
        }

        public virtual void InitializationComplete()
        {
            OnInitializationComplete();
        }

        /// <summary>
        /// To be called when <see cref="DashboardManager"/> finished the initialization of this Component (filled in required services)
        /// <para>A good place to call <see cref="Services.AuthCodeService.RequireScopes(string[])"/>.</para>
        /// </summary>
        protected virtual void OnInitialize()
        {

        }

        /// <summary>
        /// To be called when <see cref="DashboardManager"/> instantiated all components.
        /// <para>A good place to call <see cref="Services.AuthCodeService.Authorize(System.Threading.CancellationToken)"/>.</para>
        /// </summary>
        protected virtual void OnInitializationComplete()
        {

        }

        public virtual DashboardComponent Parent { get; set; }

        private bool foreground = true;

        public bool ThisForeground
        {
            get => foreground;
            set
            {
                SetAndNotify(ref foreground, value);
                ForegroundChanged();
            }
        }

        public bool Foreground
        {
            get
            {
                return ThisForeground && (Parent?.Foreground ?? true);
                //var fg = ThisForeground;
                //var comp = this;
                //while (comp.Parent != null)
                //{
                //    fg &= comp.Parent.ThisForeground;
                //    comp = comp.Parent;
                //}
                //return fg;
            }
        }

        public virtual void ForegroundChanged()
        {
            OnForegroundChanged();
        }

        protected virtual void OnForegroundChanged()
        {

        }

        public virtual void GetServices(DashboardManager manager)
        {
            var properties = GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.IsDefined(typeof(RequireServiceAttribute), true))
                .Select(prop => (prop, (RequireServiceAttribute)Attribute.GetCustomAttribute(prop, typeof(RequireServiceAttribute))));
            properties.ForEach(x => x.prop.SetValue(this, manager.GetService(x.prop.PropertyType, (string)GetType().GetProperty(x.Item2.ServiceIdProperty).GetValue(this))));
        }

        private bool loaded = false;

        /// <summary>
        /// Whether the Component has finished the initial load (including authentication and initial data load).
        /// <para>Will invoke <see cref="FinishedLoading"/> when this is set from false to true.</para>
        /// </summary>
        public bool Loaded
        {
            get => loaded;
            protected set
            {
                if (value && !loaded)
                {
                    loaded = value;
                    FinishedLoading?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    loaded = value;
                }
            }
        }

        /// <summary>
        /// When the Component finished the initial load (including authentication and initial data load).
        /// </summary>
        public event EventHandler FinishedLoading;
    }
}
