using UnityEngine;
using UnityEngine.InputSystem;

// Lê o teclado e passa os comandos para o carro do jogador.
[RequireComponent(typeof(CarController))]
public class PlayerDriver : MonoBehaviour
{
    private CarController car;

    void Awake()
    {
        car = GetComponent<CarController>();
    }

    void Update()
    {
        // Sem teclado conectado, Keyboard.current é nulo.
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            car.SetInput(0f, 0f, false, false);
            return;
        }

        float throttle = 0f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            throttle += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            throttle -= 1f;

        // Na Unity, rotação positiva é para a esquerda (sentido anti-horário).
        float steering = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            steering += 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            steering -= 1f;

        bool handbrake = keyboard.spaceKey.isPressed;
        bool nitro = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

        car.SetInput(throttle, steering, handbrake, nitro);
    }
}
