using System.Collections;
using UnityEngine;


public class ColliderInflate : MonoBehaviour
{
    public float inflateScale = 1.4f; // Factor por el cual el objeto se inflará.
    public float inflateDuration = 0.2f; // Duración del efecto de inflar.
    public float deflateDuration = 0.2f; // Duración del efecto de desinflar.
    private Vector3 originalScale; // Escala original del GameObject.

    private void Start()
    {
        originalScale = transform.localScale; // Guarda la escala original al iniciar.
    }

    //private void OnTriggerEnter(Collider other) // Para 3D
    ////private void OnTriggerEnter2D(Collider2D other) // Descomenta esta línea si estás trabajando en 2D
    //{
    //    if (other.CompareTag("StarsParticle")) // Asegúrate de que el tag de tus partículas sea "Particle"
    //    {
    //        StartCoroutine(InflateDeflate());
    //    }
    //}

    private void OnParticleCollision(GameObject other)
    {
        StartCoroutine(InflateDeflate());
    }

    private IEnumerator InflateDeflate()
    {
        // Inflar
        float timer = 0;
        while (timer <= inflateDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, originalScale * inflateScale, timer / inflateDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Esperar un momento en el tamaño inflado
        yield return new WaitForSeconds(0.1f);

        // Desinflar
        timer = 0;
        while (timer <= deflateDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale * inflateScale, originalScale, timer / deflateDuration);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
