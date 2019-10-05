﻿
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditor;

namespace Graph2
{
    public class PortView : Port
    {
        protected PortView(
            Orientation portOrientation, 
            Direction portDirection, 
            Capacity portCapacity, 
            Type type
        ) : base(portOrientation, portDirection, portCapacity, type)
        {
            // TODO: Less hardcoded of a path
            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Graph/Editor/Styles/PortView.uss"
            );
        
            styleSheets.Add(styles);
            
            visualClass = GetTypeVisualClass(type);
        }
    
        public static PortView Create(
            NodePort port, 
            Orientation portOrientation, 
            Direction portDirection, 
            SerializedProperty prop, 
            Type type,
            IEdgeConnectorListener connectorListener
        ) {
            var view = new PortView(
                portOrientation, 
                portDirection, 
                port.allowMany ? Capacity.Multi : Capacity.Single, 
                type
            ) {
                m_EdgeConnector = new EdgeConnector<Edge>(connectorListener),
                portName = port.fieldName,
                userData = port
            };
        
            view.AddManipulator(view.m_EdgeConnector);

            // Bind to the underlying field
            if (prop != null)
            {
                var field = new PropertyField(prop, " ");
                field.Bind(prop.serializedObject);
                field.RegisterCallback<ChangeEvent<string>>((evt) =>
                {
                    Debug.Log(evt);
                });

                view.m_ConnectorBox.parent.Add(field);
            }

            return view;
        }

        /// <summary>
        /// Return true if this port can be connected with an edge to the given port
        /// </summary>
        public bool IsCompatibleWith(PortView other)
        {
            // Note: direction should be account for here as well. And possibly
            // any type of loop detection to ensure nobody is making a cycle 
            // (for certain use cases, that is)

            // For now, just make it exact based on type classification
            return visualClass == other.visualClass;
        }

        public string GetTypeVisualClass(Type type)
        {
            // TODO: Better variant that handles lists and such.

            if (type.IsEnum)
            {
                return "type-System-Enum";
            }

            return "type-" + type.FullName.Replace(".", "-");
        }
    }
}