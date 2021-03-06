using System;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// Extension methods for <see cref="VisualElement"/>.
    /// </summary>
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Removes all USS classes that start with <paramref name="classNamePrefix"/> and add
        /// a USS class name <paramref name="classNamePrefix"/> + <paramref name="classNameSuffix"/>.
        /// </summary>
        /// <param name="ve">The VisualElement to act upon.</param>
        /// <param name="classNamePrefix">The class name prefix.</param>
        /// <param name="classNameSuffix">The class name suffix.</param>
        public static void PrefixEnableInClassList(this VisualElement ve, string classNamePrefix, string classNameSuffix)
        {
            var toRemove = ve.GetClasses().Where(c => c.StartsWith(classNamePrefix)).ToList();

            var className = classNamePrefix + classNameSuffix;
            var classAlreadyPresent = toRemove.Remove(className);

            if (toRemove.Count > 0)
            {
                foreach (var c in toRemove)
                {
                    ve.RemoveFromClassList(c);
                }
            }

            if (!classAlreadyPresent)
                ve.AddToClassList(className);
        }

        /// <summary>
        /// Replaces a manipulator by another one.
        /// </summary>
        /// <param name="ve">The VisualElement to act upon.</param>
        /// <param name="manipulator">The manipulator to remove.</param>
        /// <param name="newManipulator">The manipulator to add.</param>
        /// <typeparam name="T">The type of the manipulators.</typeparam>
        public static void ReplaceManipulator<T>(this VisualElement ve, ref T manipulator, T newManipulator) where T : Manipulator
        {
            ve.RemoveManipulator(manipulator);
            manipulator = newManipulator;
            ve.AddManipulator(newManipulator);
        }

        /// <summary>
        /// Get the rectangle representing the size of this VisualElement, with origin at (0,0).
        /// </summary>
        /// <param name="ve">The VisualElement for which we want to get the dimensions.</param>
        /// <returns>A rectangle representing the size of this VisualElement, with origin at (0,0)</returns>
        public static Rect GetRect(this VisualElement ve)
        {
            return new Rect(0.0f, 0.0f, ve.layout.width, ve.layout.height);
        }

        /// <summary>
        /// Queries a VisualElement for a descendant that matches some criteria. Same as VisualElement.Q(),
        /// but behaves better when <paramref name="e"/> is null.
        /// </summary>
        /// <param name="e">The VisualElement to search.</param>
        /// <param name="name">The name of the descendant to find. Null if this criterion should be ignored.</param>
        /// <param name="className">The USS class name of the descendant to find. Null if this criterion should be ignored.</param>
        /// <typeparam name="T">The type of the descendant to find.</typeparam>
        /// <returns>The VisualElement that matches the search criteria.</returns>
        public static T SafeQ<T>(this VisualElement e, string name = null, string className = null) where T : VisualElement
        {
            return e?.Q<T>(name, className);
        }

        /// <summary>
        /// Queries a VisualElement for a descendant that matches some criteria. Same as VisualElement.Q(),
        /// but behaves better when <paramref name="e"/> is null.
        /// </summary>
        /// <param name="e">The VisualElement to search.</param>
        /// <param name="name">The name of the descendant to find. Null if this criterion should be ignored.</param>
        /// <param name="className">The USS class name of the descendant to find. Null if this criterion should be ignored.</param>
        /// <returns>The VisualElement that matches the search criteria.</returns>
        public static VisualElement SafeQ(this VisualElement e, string name = null, string className = null)
        {
            return e?.Q(name, className);
        }

        /// <summary>
        /// Queries a VisualElement for a descendant that matches some criteria. Same as VisualElement.SafeQ(),
        /// but throws when no element is found or <paramref name="e"/> is null.
        /// </summary>
        /// <param name="e">The VisualElement to search.</param>
        /// <param name="name">The name of the descendant to find. Null if this criterion should be ignored.</param>
        /// <param name="className">The USS class name of the descendant to find. Null if this criterion should be ignored.</param>
        /// <returns>The VisualElement that matches the search criteria.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="e"/> is null.</exception>
        /// <exception cref="Exception">If no element is found.</exception>
        public static T MandatoryQ<T>(this VisualElement e, string name = null, string className = null) where T : VisualElement
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            return GraphViewStaticBridge.MandatoryQ<T>(e, name, className);
        }

        /// <summary>
        /// Queries a VisualElement for a descendant that matches some criteria. Same as VisualElement.SafeQ(),
        /// but throws when no element is found or <paramref name="e"/> is null.
        /// </summary>
        /// <param name="e">The VisualElement to search.</param>
        /// <param name="name">The name of the descendant to find. Null if this criterion should be ignored.</param>
        /// <param name="className">The USS class name of the descendant to find. Null if this criterion should be ignored.</param>
        /// <returns>The VisualElement that matches the search criteria.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="e"/> is null.</exception>
        /// <exception cref="Exception">If no element is found.</exception>
        public static VisualElement MandatoryQ(this VisualElement e, string name = null, string className = null)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            return GraphViewStaticBridge.MandatoryQ(e, name, className);
        }
    }
}
