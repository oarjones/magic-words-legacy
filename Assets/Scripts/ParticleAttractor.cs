using UnityEngine;

public class ParticleAttractor : MonoBehaviour
{
    public ParticleSystem particleSys;
    public Transform target;

    public ParticleSystem particleSysOpponent;
    public Transform targetOpponent;

    public float speed = 5f;
    //public float curveMagnitude = 0.1f; // Magnitud de la curva en la animación

    private ParticleSystem.Particle[] particles;

    private ParticleSystem.Particle[] particlesOpponent;
    //private Vector3[] offsets; // Almacena los vectores de desplazamiento aleatorios para cada partícula

    void Start()
    {
        int maxParticles = particleSys.main.maxParticles;
        particles = new ParticleSystem.Particle[maxParticles];

        int maxParticlesOpponent = particleSysOpponent.main.maxParticles;
        particlesOpponent = new ParticleSystem.Particle[maxParticlesOpponent];
        //offsets = new Vector3[maxParticles];

        // Inicializar el desplazamiento aleatorio para cada partícula
        //for (int i = 0; i < maxParticles; i++)
        //{
        //    offsets[i] = new Vector3(
        //        Random.Range(-curveMagnitude, curveMagnitude),
        //        Random.Range(-curveMagnitude, curveMagnitude),
        //        Random.Range(-curveMagnitude, curveMagnitude)
        //    );
        //}
    }

    void LateUpdate()
    {
        int particleCount = particleSys.GetParticles(particles);
        float step = speed * Time.deltaTime;

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 particleWorldPosition = particleSys.main.simulationSpace == ParticleSystemSimulationSpace.Local
                ? particleSys.transform.TransformPoint(particles[i].position)
                : particles[i].position;

            Vector3 directionToTarget = (target.position - particleWorldPosition).normalized;

            // Agregar una curva a la trayectoria
            //Vector3 curve = offsets[i] * Mathf.Sin(Time.time * speed);
            Vector3 newPosition = particleWorldPosition + (directionToTarget * step);

            particles[i].position = particleSys.main.simulationSpace == ParticleSystemSimulationSpace.Local
                ? particleSys.transform.InverseTransformPoint(newPosition)
                : newPosition;
        }

        // Aplicar los cambios a las partículas
        particleSys.SetParticles(particles, particleCount);








        particleCount = particleSysOpponent.GetParticles(particlesOpponent);
        step = speed * Time.deltaTime;

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 particleWorldPosition = particleSysOpponent.main.simulationSpace == ParticleSystemSimulationSpace.Local
                ? particleSysOpponent.transform.TransformPoint(particlesOpponent[i].position)
                : particlesOpponent[i].position;

            Vector3 directionToTarget = (targetOpponent.position - particleWorldPosition).normalized;

            // Agregar una curva a la trayectoria
            //Vector3 curve = offsets[i] * Mathf.Sin(Time.time * speed);
            Vector3 newPosition = particleWorldPosition + (directionToTarget * step);

            particlesOpponent[i].position = particleSysOpponent.main.simulationSpace == ParticleSystemSimulationSpace.Local
                ? particleSysOpponent.transform.InverseTransformPoint(newPosition)
                : newPosition;
        }

        // Aplicar los cambios a las partículas
        particleSysOpponent.SetParticles(particlesOpponent, particleCount);



    }
}
