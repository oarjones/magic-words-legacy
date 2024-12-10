using UnityEngine;
using TMPro;
using DG.Tweening; // Asegúrate de tener DOTween en tu proyecto

public class DialogBubble : MonoBehaviour
{
    public TextMeshProUGUI textComponent; // Asigna tu componente de texto TMP en el inspector
    public RectTransform backgroundRectTransform; // Asigna el RectTransform del fondo del globo
    public GameObject dialogBubbleObject; // Asigna el objeto completo del globo de diálogo

    private void Awake()
    {
        // Asegúrate de que el globo de diálogo esté inactivo al inicio
        dialogBubbleObject.SetActive(false);
    }

    // Inicia la animación del mensaje
    public void ShowMessage(string message, float duration = 3.0f)
    {
        dialogBubbleObject.SetActive(true); // Activa el objeto del globo de diálogo

        textComponent.text = message;
        // Ajusta el tamaño del fondo según el tamaño del mensaje
        //float textPaddingSize = 4f;
        //Vector2 backgroundSize = new Vector2(textComponent.preferredWidth + textPaddingSize * 2, textComponent.preferredHeight + textPaddingSize * 2);
        //backgroundRectTransform.sizeDelta = backgroundSize;

        // Inicia desde escalado a 0 para hacerlo "crecer"
        backgroundRectTransform.localScale = Vector3.zero;
        textComponent.transform.localScale = Vector3.zero; // Asegúrate de que el texto también comience a escalar desde 0

        // Animación de aparición
        Sequence mySequence = DOTween.Sequence(); // Crear una secuencia para animar ambos elementos juntos
        mySequence.Append(backgroundRectTransform.DOScale(1, 0.5f).SetEase(Ease.OutBack));
        mySequence.Join(textComponent.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack)); // Animar el texto junto con el fondo

        // Ocultar después de un tiempo y luego desactivar
        DOVirtual.DelayedCall(duration, () =>
        {
            Sequence myCloseSequence = DOTween.Sequence(); // Crear otra secuencia para la animación de salida
            myCloseSequence.Append(backgroundRectTransform.DOScale(0, 0.5f).SetEase(Ease.InBack));
            myCloseSequence.Join(textComponent.transform.DOScale(0, 0.5f).SetEase(Ease.InBack));
            //myCloseSequence.OnComplete(() => dialogBubbleObject.SetActive(false)); // Desactiva el objeto al completar la animación de ocultar
        });
    }

}
