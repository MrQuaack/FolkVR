using UnityEngine;
using DG.Tweening;

public class MenuTransitionManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public RectTransform mainMenu;
    public RectTransform subMenu;

    [Header("Animation Settings")]
    public float transitionDuration = 0.5f;
    public float bounceScale = 1.1f;

    private Vector2 mainMenuOriginalPos;
    private Vector2 subMenuOriginalPos;

    void Start()
    {
        // Store original positions
        mainMenuOriginalPos = mainMenu.anchoredPosition;
        subMenuOriginalPos = subMenu.anchoredPosition;

        // Initialize subMenu off-screen
        subMenu.anchoredPosition = new Vector2(0, -Screen.height);
        subMenu.localScale = Vector3.one; // Ensure scale is reset
    }

    public void ShowSubMenu()
    {

         // Kill any existing tweens on mainMenu and subMenu
        mainMenu.DOKill();
        subMenu.DOKill();

        // Move mainMenu off-screen
        mainMenu.DOAnchorPos(new Vector2(0, Screen.height), transitionDuration)
            .SetEase(Ease.InBack);

        // Move subMenu into view
        subMenu.DOAnchorPos(subMenuOriginalPos, transitionDuration)
            .SetEase(Ease.OutBack);

        // Apply bounce effect to subMenu
        subMenu.DOScale(bounceScale, transitionDuration / 2)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() =>
            {
                // Ensure scale is reset after bounce
                subMenu.localScale = Vector3.one;
            });
    }

    public void ShowMainMenu()
    {
        // Kill any existing tweens on mainMenu and subMenu
        mainMenu.DOKill();
        subMenu.DOKill();

        // Move subMenu off-screen
        subMenu.DOAnchorPos(new Vector2(0, -Screen.height), transitionDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Debug.Log("SubMenu moved off-screen.");
                subMenu.anchoredPosition = new Vector2(0, -Screen.height); // Ensure final position
            });

        // Move mainMenu into view
        mainMenu.DOAnchorPos(mainMenuOriginalPos, transitionDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Debug.Log("MainMenu animation completed.");
                mainMenu.localScale = Vector3.one; // Ensure scale is reset
            });

        // Apply bounce effect to mainMenu
        mainMenu.DOScale(bounceScale, transitionDuration / 2)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() =>
            {
                mainMenu.localScale = Vector3.one; // Ensure scale is reset
            });
    }
}