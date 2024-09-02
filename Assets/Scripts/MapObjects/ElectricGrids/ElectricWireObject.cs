// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
//
// using Assets.Scripts.MapObjects;
//
// using UnityEngine;
//
// namespace MapObjects.ElectricGrids
// {
//     [RequireComponent(typeof(BoxCollider2D))]
//     public class ElectricWireObject : MapObject
//     {
//         public (IPowerGridUser, IPowerGridUser) ConnectedGridUsers { get; set; }
//
//         public bool CanConnect()
//         {
//             return true;
//         }
//
//         public bool HasPower()
//         {
//             return ConnectedGridUsers.Any(x => x != null && x.HasPower());
//         }
//
//         public bool IsConnected()
//         {
//             return ConnectedGridUsers.Any();
//         }
//
//         protected override void OnPlace()
//         {
//             base.OnPlace();
//
//             ConnectedGridUsers = new List<IPowerGridUser>();
//         }
//
//         private IEnumerator WaitForAction()
//         {
//             while (true)
//             {
//                 if (IsExitKeyClicked())
//                 {
//                     break;
//                 }
//
//                 if (IsMouseButtonClicked())
//                 {
//                     break;
//                 }
//
//                 yield return null;
//             }
//         }
//
//         private bool IsExitKeyClicked()
//         {
//             return Input.GetKey(KeyCode.Escape);
//         }
//
//         private bool IsMouseButtonClicked()
//         {
//             if (Input.GetMouseButtonDown(0) && _camera != null)
//             {
//                 var ray = _camera.ScreenPointToRay(Input.mousePosition);
//
//                 if (Physics.Raycast(ray, out var hit))
//                 {
//                     return HandleClickOnPowerGridUser(hit);
//                 }
//             }
//
//             return false;
//         }
//
//         private bool HandleClickOnPowerGridUser(RaycastHit hit)
//         {
//             var targetGridUser = hit.collider.gameObject.GetComponent<IPowerGridUser>();
//
//             if (targetGridUser == null)
//             {
//                 return false;
//             }
//
//             targetGridUser.ConnectUsers(this);
//
//             return true;
//         }
//     }
// }
