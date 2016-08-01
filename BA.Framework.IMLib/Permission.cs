using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    /// <summary>
    /// 权限处理相关
    /// </summary>
    internal class Permission
    {
        public static List<UserPermission> GetUserPermission(string permission)
        {
            List<UserPermission> userPermissionList = new List<UserPermission>();
            int permissionInt = 0;
            if (string.IsNullOrEmpty(permission) ||
                !int.TryParse(permission, out permissionInt))
            {
                return userPermissionList;
            }

            IEnumerable<int> values = Enum.GetValues(typeof(UserPermission)).Cast<int>();
            foreach (var item in values)
            {
                if ((item & permissionInt) > 0)
                {
                    userPermissionList.Add((UserPermission)item);
                }
            }
            return userPermissionList;
        }

        internal static bool CheckPermission(UserIdentity m_User, MessageType type)
        {
            //待服务器做好权限后再开放
            return true;

            if (!m_User.IsAuthenticated || m_User.PermissionList == null || m_User.PermissionList.Count == 0)
            {
                return false;
            }

            UserPermission permission = UserPermission.NO_PERMISSION;
            if (m_PermissionMapping.Keys.Contains(type))
            {
                permission = m_PermissionMapping[type];
            }
            else
            {
                int matchCount = m_User.PermissionList.Count(x => x.ToString().ToLower().Contains(type.ToString().ToLower()));
                if (matchCount == 1)
                {
                    permission = m_User.PermissionList.FirstOrDefault(x => x.ToString().ToLower().Contains(type.ToString().ToLower()));
                }
                else//找不到匹配类型
                    return false;
            }

            if (m_User.PermissionList.Contains(permission))
            {
                return true;
            }
            return false;
        }
        private static Dictionary<MessageType, UserPermission> m_PermissionMapping = new Dictionary<MessageType, UserPermission>();
        static Permission()
        {
            m_PermissionMapping.Add(MessageType.Text, UserPermission.SEND_TEXT);
        }
        //private static UserPermission GetMappedPermissionByMessageType(MessageType type)
        //{
        //    switch (type)
        //    {
        //        case MessageType.Ack:
        //            break;
        //        case MessageType.Connect:
        //            break;
        //        case MessageType.Disconnect:
        //            break;
        //        case MessageType.Ping:
        //            break;
        //        case MessageType.Text:
        //            break;
        //        case MessageType.Image:
        //            break;
        //        case MessageType.Voice:
        //            break;
        //        case MessageType.Video:
        //            break;
        //        case MessageType.File:
        //            break;
        //        case MessageType.Invite:
        //            break;
        //        case MessageType.Join:
        //            break;
        //        case MessageType.Leave:
        //            break;
        //        case MessageType.undo:
        //            break;
        //        case MessageType.Transfer:
        //            break;
        //        case MessageType.Link:
        //            break;
        //        case MessageType.Custom:
        //            break;
        //        case MessageType.UnKnown:
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
}
