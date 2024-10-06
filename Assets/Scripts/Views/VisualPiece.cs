using UnityEngine;
using DG.Tweening;

namespace Game.Match3.ViewComponents
{

    [RequireComponent(typeof(SpriteRenderer))]
    public class VisualPiece : MonoBehaviour
    {
        [Header("Shake Animation settings")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeStrength = 0.1f;
        [SerializeField] private int vibrate = 10;

        [Space(15)]
        [Header("Jump Animation Settings")]
        [SerializeField] private float jumpPower = 1;
        [SerializeField] private int numOfJumps = 1;
        [SerializeField] private float duration = 0.2f;

        public void SetSprite(Sprite sprite)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
        }
        public void DOShakePosition()
        {
            transform.DOShakePosition(shakeDuration, shakeStrength, vibrate);
        }
        public void DoJump()
        {
            transform.DOJump(transform.localPosition, jumpPower, numOfJumps, duration)
                    .SetEase(Ease.OutQuad);
     
        }

        public void DOScale(float compressScale = 0.8f, float duration = 0.5f)
        {
            Vector3 originalScale = transform.localScale;

            transform.DOScale(originalScale * compressScale, duration / 2f)
                .SetEase(Ease.InOutBounce) 
                .OnComplete(() =>
                {
                    transform.DOScale(originalScale, duration / 2f) 
                        .SetEase(Ease.OutQuad); 
                });
        }
    }
}