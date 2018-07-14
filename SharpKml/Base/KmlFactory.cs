﻿// Copyright (c) Samuel Cragg.
//
// Licensed under the MIT license. See LICENSE file in the project root for
// full license information.

namespace SharpKml.Base
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using SharpKml.Dom;

    /// <summary>
    /// Creates a derived class of <see cref="Element"/> from an XML element.
    /// </summary>
    public static class KmlFactory
    {
        private static readonly Dictionary<Type, XmlComponent> Names = new Dictionary<Type, XmlComponent>();

        // We need two so we can do reverse lookups
        private static readonly Dictionary<XmlComponent, Type> Types = new Dictionary<XmlComponent, Type>();

        /// <summary>
        /// Initializes static members of the <see cref="KmlFactory"/> class.
        /// </summary>
        static KmlFactory()
        {
            // Register all the sub-classes of Element that are
            // in the same assembly as Element.
            RegisterAssembly(typeof(Element).GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Creates a derived class of <see cref="Element"/> based on the XML
        /// information.
        /// </summary>
        /// <param name="xml">The XML information of the element.</param>
        /// <returns>
        /// A derived class of <c>Element</c> if the specified information was
        /// found; otherwise, null.
        /// </returns>
        /// <exception cref="System.MemberAccessException">
        /// Cannot create an instance of an abstract class, or this member was
        /// invoked with a late-binding mechanism.
        /// </exception>
        /// <exception cref="System.Reflection.TargetInvocationException">
        /// The constructor being called throws an exception.
        /// </exception>
        public static Element CreateElement(XmlComponent xml)
        {
            if (Types.TryGetValue(xml, out Type type))
            {
                return (Element)Activator.CreateInstance(type);
            }

            return null;
        }

        /// <summary>
        /// Gets the XML information associated with the specified type.
        /// </summary>
        /// <param name="type">The type to search for.</param>
        /// <returns>
        /// An XmlComponent containing the XML information associated with the
        /// specified type if the type was found; otherwise, null.
        /// </returns>
        /// <exception cref="ArgumentNullException">type is null.</exception>
        public static XmlComponent FindType(Type type)
        {
            // Will throw if type is null.
            if (Names.TryGetValue(type, out XmlComponent value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Registers the specified type with the specified XML name and
        /// namespace URI.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the class to be registered, deriving from
        /// <see cref="Element"/>.
        /// </typeparam>
        /// <param name="xml">The XML information of the element.</param>
        /// <exception cref="ArgumentNullException">xml is null.</exception>
        /// <exception cref="ArgumentException">
        /// The type has already been registered or another type with the
        /// same XML name and namespace URI has been already registered.
        /// </exception>
        public static void Register<T>(XmlComponent xml)
            where T : Element
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            RegisterType(xml.Clone(), typeof(T)); // Don't store what the user passed us
        }

        private static void RegisterAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.ExportedTypes)
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                if (typeInfo.IsSubclassOf(typeof(Element)))
                {
                    KmlElementAttribute element = TypeBrowser.GetElement(typeInfo);
                    if (element != null)
                    {
                        var xml = new XmlComponent(null, element.ElementName, element.Namespace);
                        RegisterType(xml, type);
                    }
                }
            }
        }

        // Private helper function to ensure both dictionaries are updated.
        private static void RegisterType(XmlComponent xml, Type type)
        {
            if (Names.ContainsKey(type))
            {
                throw new ArgumentException("Class type has already been registered.");
            }

            if (Types.ContainsKey(xml))
            {
                throw new ArgumentException("Another type has been registered with the specified XML qualified name.");
            }

            Names.Add(type, xml);
            Types.Add(xml, type);
        }
    }
}
