using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System;

public class SandClock : MonoBehaviour
{
	[SerializeField] Image fillTopImage;
	[SerializeField] Image fillBottomImage;
	[SerializeField] Image sandDotsImage;
	[SerializeField] RectTransform sandPyramidRect;


	[Space (30f)]
	public float roundDuration = 10f;

	float defaultSandPyramidYPos;
	Quaternion defaultClockRotation;

	void Awake ()
	{
		defaultSandPyramidYPos = sandPyramidRect.anchoredPosition.y;
		defaultClockRotation = transform.localRotation;
        sandPyramidRect.localScale = new Vector3(sandPyramidRect.localScale.x, 0, sandPyramidRect.localScale.z);
        sandDotsImage.DOFade (0f, 0f);
	}

	public void Begin ()
	{
        transform.localRotation = defaultClockRotation; // Restaurar la rotación inicial

        transform
            .DORotate(new Vector3(0f, 0f, 720f), 1.6f, RotateMode.FastBeyond360) // Se usan valores de euler para la rotación
            .SetRelative(true) // Rotación relativa al estado actual
            .SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                StartTimer();
            });



    }

    private void StartTimer()
    {

        sandDotsImage.DOFade(1f, .8f);
        sandDotsImage.material.DOOffset(Vector2.down * -roundDuration, roundDuration).From(Vector2.zero).SetEase(Ease.Linear);

        //Scale Pyramid
        sandPyramidRect.DOScaleY(1f, roundDuration / 3f).From(0f);
        sandPyramidRect.DOScaleY(0f, roundDuration / 1.5f).SetDelay(roundDuration / 3f).SetEase(Ease.Linear);

        sandPyramidRect.anchoredPosition = Vector2.up * defaultSandPyramidYPos;
        sandPyramidRect.DOAnchorPosY(0f, roundDuration).SetEase(Ease.Linear);

        ResetClock();

        //roundText.DOFade (1f, .8f);

        fillTopImage
            .DOFillAmount(0, roundDuration)
            .SetEase(Ease.Linear)
            .OnUpdate(OnTimeUpdate)
            .OnComplete(OnRoundTimeComplete);
    }

    void OnTimeUpdate ()
	{
		fillBottomImage.fillAmount = 1f - fillTopImage.fillAmount;
	}

	void OnRoundTimeComplete ()
	{
		sandDotsImage.DOFade (0f, 0f);
		transform.DOShakeScale (.8f, .3f, 10, 90f, true);

        GameEvents.OnEndTimerCountdownMethod();

    }


	public void ResetClock ()
	{
		transform.rotation = defaultClockRotation; //Quaternion.Euler (Vector3.zero);
        fillTopImage.fillAmount = 1f;
		fillBottomImage.fillAmount = 0f;
        

    }
}
