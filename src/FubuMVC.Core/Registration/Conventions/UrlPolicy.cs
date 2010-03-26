using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuCore.Reflection;
using FubuCore.Util;
using FubuMVC.Core.Diagnostics;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;

namespace FubuMVC.Core.Registration.Conventions
{
    public class UrlPolicy : IUrlPolicy
    {
        private readonly Func<ActionCall, bool> _filter;
        private readonly List<Type> _ignoreClassNames = new List<Type>();
        private readonly List<string> _ignoredClassSuffixes = new List<string>();
        private readonly List<string> _ignoredMethodNames = new List<string>();
        private readonly List<string> _ignoredNamespaces = new List<string>();

        private readonly Builder<MethodInfo, string> _methodNameBuilder
            = new Builder<MethodInfo, string>(method => method.Name);

        private readonly IRouteInputPolicy _routeInputPolicy;
        private readonly Builder<ActionCall, string> _appendClassBuilder = new Builder<ActionCall, string>(call => "");

        public UrlPolicy(Func<ActionCall, bool> filter, IRouteInputPolicy routeInputPolicy)
        {
            _filter = filter;
            _routeInputPolicy = routeInputPolicy;
        }

        public bool IgnoreControllerFolderName { get; set; }
        public bool IgnoreControllerNamespaceEntirely { get; set; }
        public bool IgnoreControllerNamesEntirely { get; set; }

        public bool Matches(ActionCall call, IConfigurationObserver log)
        {
            return _filter(call);
        }

        public IRouteDefinition Build(ActionCall call)
        {
            IRouteDefinition route = call.ToRouteDefinition();

            if (! IgnoreControllerNamespaceEntirely)
            {
                addNamespace(route, call);
            }

            if(! IgnoreControllerNamesEntirely)
                addClassName(route, call);

            addMethodName(route, call);

            if (call.HasInput)
            {
                _routeInputPolicy.AlterRoute(route, call);
            }


            return route;
        }

        private void addMethodName(IRouteDefinition route, ActionCall call)
        {
            if (_ignoredMethodNames.Contains(call.Method.Name.ToLower())) return;
            string urlPart = _methodNameBuilder.Build(call.Method);
            if (urlPart.IsNotEmpty())
            {
                route.Append(urlPart.ToLower());
            }
        }

        private void addClassName(IRouteDefinition route, ActionCall call)
        {
            if (_ignoreClassNames.Contains(call.HandlerType)) return;

            string className = getClassName(call);

            // So Home/HomeController == /home and not /home/home
            string lastName = route.Pattern.Split('/').LastOrDefault();
            if (className != lastName)
            {
                className += _appendClassBuilder.Build(call);
                route.Append(className);
            }
        
        }

        private string getClassName(ActionCall call)
        {
            string returnValue = null;
            call.HandlerType.ForAttribute<UrlFolderAttribute>(x => returnValue = x.Folder);

            return returnValue ?? replace(call.HandlerType.Name, _ignoredClassSuffixes);
        }

        private void addNamespace(IRouteDefinition route, ActionCall call)
        {
            string ns = replace(call.HandlerType.Namespace, _ignoredNamespaces).TrimStart('.');
            ns = ns.Replace('.', '/');
            route.Append(ns);

            if (IgnoreControllerFolderName)
            {
                route.RemoveLastPatternPart();
            }
        }

        private string replace(string starting, List<string> list)
        {
            string returnValue = starting.ToLower();
            list.Each(x => returnValue = returnValue.Replace(x, ""));

            return returnValue;
        }

        public void IgnoreNamespace(string nameSpace)
        {
            _ignoredNamespaces.Add(nameSpace.ToLower());
        }

        public void IgnoreMethods(string name)
        {
            _ignoredMethodNames.Add(name.ToLower());
        }

        public void IgnoreMethodSuffix(string suffix)
        {
            RegisterMethodNameStrategy(m => m.Name.EndsWith(suffix), m => m.Name.Replace(suffix, ""));
        }

        public void IgnoreClassSuffix(string suffix)
        {
            _ignoredClassSuffixes.Add(suffix.ToLower());
        }

        public void IgnoreClassName(Type type)
        {
            _ignoreClassNames.Add(type);
        }

        public void AppendClassesWith(Func<ActionCall, bool> filter, string pattern)
        {
            _appendClassBuilder.Register(filter, call => pattern);
        }

        public void RegisterMethodNameStrategy(Func<MethodInfo, bool> filter, Func<MethodInfo, string> builder)
        {
            _methodNameBuilder.Register(filter, builder);
        }
    }
}