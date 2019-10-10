﻿
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using BlueGraph;

namespace BlueGraphEditor
{
    /// <summary>
    /// Wrapper over GraphView.Group to pass changes onto 
    /// the serializable asset
    /// </summary>
    public class GroupView : Group, ICanDirty
    {
        // TODO: Less dumb theme names
        public enum Theme
        {
            Dark,
            Light,
            Info,
            Danger
        }

        public NodeGroup target;
        
        Theme m_Theme;

        public GroupView(NodeGroup group)
        {
            target = group;
            title = group.title;
            
            // TODO: Less hardcoded of a path
            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/BlueGraph/Editor/Styles/GroupView.uss"
            );
        
            styleSheets.Add(styles);
            
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            var text = new TextField();
            text.multiline = true;
            text.AddToClassList("group-comment");
            Add(text);


            SetTheme(Theme.Dark);
        }
        
        public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is GroupView)
            {
                // Add options to change theme
                foreach (var theme in (Theme[])Enum.GetValues(typeof(Theme)))
                {
                    evt.menu.AppendAction(
                        theme + " Theme", 
                        (a) => { SetTheme(theme); }, 
                        (m_Theme != theme) ? DropdownMenuAction.Status.Normal 
                            : DropdownMenuAction.Status.Disabled
                    );
                }

                evt.menu.AppendSeparator();
            }
        }

        public void SetTheme(Theme theme)
        {
            RemoveFromClassList("theme-" + m_Theme);
            AddToClassList("theme-" + theme);
            m_Theme = theme;
        }

        public virtual void OnDirty()
        {
            
        }

        public virtual void OnUpdate()
        {

        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (var element in elements)
            {
                if (element is NodeView)
                {
                    var node = (element as NodeView).target;
                    if (!target.nodes.Contains(node))
                    {
                        target.nodes.Add(node);
                    }
                }
            }

            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {   
            foreach (var element in elements)
            {
                if (element is NodeView)
                {
                    var node = (element as NodeView).target;
                    target.nodes.Remove(node);
                }
            }

            base.OnElementsRemoved(elements);
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            base.OnGroupRenamed(oldName, newName);
            
            // Force the group to have a title if cleared. This avoids awkward
            // interactions when trying to move the group or add a title later.
            if (newName.Length < 1)
            {
                newName = "New Group";
            }

            target.title = newName;
            title = newName;
        }
    }
}
