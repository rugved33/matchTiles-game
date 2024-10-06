using UnityEngine;
using DG.Tweening;

namespace Game.Match3.ViewComponents
{

    [RequireComponent(typeof(SpriteRenderer))]
    public class VisualPiece : MonoBehaviour
    {

        public void SetSprite(Sprite sprite)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
        }
        public void DOShakePosition()
        {
            transform.DOShakePosition(0.5f, 0.1f, 10);
        }
        public void DoJump()
        {
            transform.DOJump(transform.localPosition, 1f, 1, 0.2f)
                    .SetEase(Ease.OutQuad);
        }
    }
}