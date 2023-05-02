// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.InputSystem;
//
// namespace VitoBarra.System.Interaction
// {
//     public class CommandExecutes
//     {
//         private List<Key> PressedThisFrame;
//
//         public CommandExecutes()
//         {
//             PressedThisFrame = new List<Key>();
//
//             RegisterEvent();
//         }
//
//         private void RegisterEvent()
//         {
//             InputSystem.onBeforeUpdate += GetPressedKeys;
//         }
//
//         public void UnregisterEvent()
//         {
//             InputSystem.onBeforeUpdate -= GetPressedKeys;
//         }
//
//         void GetPressedKeys()
//         {
//             PressedThisFrame.Clear();
//             foreach (var key in Keyboard.current.allKeys.Where(key => key.isPressed))
//             {
//                 PressedThisFrame.Add(key.keyCode);
//             }
//         }
//
//         public void Execute()
//         {
//         }
//     }
//
//
//     public class CommandExecutor
//     {
//         private List<KeyGroupPressEvent> _events;
//         private List<Key> KeyCollection;
//
//         public CommandExecutor(List<Key> keyCollection)
//         {
//             KeyCollection = keyCollection;
//         }
//     }
//
//     public class KeyGroupPressEvent
//     {
//         public event Action OnOneKeyPress ;
//     }
// }