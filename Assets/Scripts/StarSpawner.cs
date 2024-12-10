using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class StarSpawner : MonoBehaviour
{
    public GameObject starPrefab; // Asigna el prefab de la estrella en el inspector
    public Transform target; // Asigna el objeto de UI de puntuación (score) en el inspector
    public Transform source; // Asigna el objeto de UI de puntuación (score) en el inspector
    public Transform parent;
    public float spawnDuration = 2.0f; // Duración total de la animación
    public float initialAlpha = 0.8f;
    public float initialStarsNumber = 5;
    public float maxScaleFloat = 2;
    public float rotationSpeed = 360;
    private float _starsNumber = 5;

    private void Start()
    {
        //StartCoroutine(SpawnStars());
    }

    public IEnumerator SpawnStars(float? starsnumber = null)
    {
        _starsNumber = starsnumber.HasValue ? starsnumber.Value : initialStarsNumber;
        while (_starsNumber > 0) // Loop que permite que las estrellas se instancien continuamente o según sea necesario
        {
            GameObject star = Instantiate(starPrefab, source.position, Quaternion.identity);
            StartCoroutine(MoveAndFade(star));
            _starsNumber--;
            yield return new WaitForSeconds(0.3f); // Espera antes de instanciar la siguiente estrella
        }
    }

    private IEnumerator MoveAndFade(GameObject star)
    {
        float startTime = Time.time;
        Vector3 startPosition = star.transform.position;
        Quaternion startRotation = star.transform.rotation;
        Vector3 originalScale = star.transform.localScale;
        Vector3 maxScale = originalScale * maxScaleFloat;  // Escala doble

        while (Time.time - startTime < spawnDuration)
        {
            var starRenderer = star.GetComponent<Renderer>();
            float t = (Time.time - startTime); // spawnDuration;
            float tScaled = t * maxScaleFloat;  // Escala el tiempo para el efecto de escala

            // Posición
            star.transform.position = Vector3.Lerp(startPosition, target.position, t);

            // Escala
            if (t <= 0.7f)  // Primera mitad de la animación
            {
                star.transform.localScale = Vector3.Lerp(originalScale, maxScale, tScaled);

                if (starRenderer != null)
                {
                    Color color = starRenderer.material.color;
                    color.a = Mathf.Lerp(initialAlpha, 1, t); // Ajusta la transparencia
                    starRenderer.material.color = color;
                }
            }
            else // Segunda mitad de la animación
            {
                star.transform.localScale = Vector3.Lerp(maxScale, originalScale, tScaled - 1f);
                if (starRenderer != null)
                {
                    Color color = starRenderer.material.color;
                    color.a = Mathf.Lerp(1f, 0.6f, t); // Ajusta la transparencia
                    starRenderer.material.color = color;
                }
            }

            // Rotación
            //star.transform.rotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0, 0, 360 * t), t);
            star.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            // Transparencia (asegúrate de que tu estrella tenga un componente Renderer que pueda controlar la alfa)



            yield return null;
        }

        Destroy(star);
    }
}

