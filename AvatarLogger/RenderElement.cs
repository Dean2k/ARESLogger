using Il2CppSystem.Collections.Generic;
using UnityEngine;
using VRC.Core;

public static class renderer
{
    //Renders favorited avatars from ARES favorites
    public static void StartRenderElementsCoroutine(this UiVRCList instance, List<ApiAvatar> avatarList, int offset = 0, bool endOfPickers = true, VRCUiContentButton contentHeaderElement = null)
    {
        bool flag = !instance.gameObject.activeInHierarchy || ! instance.isActiveAndEnabled ||  instance.isOffScreen || !instance.enabled;
        if (!flag)
        {
            bool flag2 = instance.scrollRect != null;
            if (flag2)
            {
                instance.scrollRect.normalizedPosition = new Vector2(0f, 0f);
            }
            instance.Method_Protected_Void_List_1_T_Int32_Boolean_VRCUiContentButton_0<ApiAvatar>(avatarList, offset, endOfPickers, contentHeaderElement);
        }
    }
}