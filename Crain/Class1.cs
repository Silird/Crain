
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
public sealed class Program : MyGridProgram
{
    // НАЧАЛО СКРИПТА
    /*
     * Скрипт для управления через кабину или удалённо конструкцией из роторов и поршней, созвая из них подобие крана для выполнения любых работ в зависимосте от инструмента на конце крана
     * Движение поршней определяется относительно направления кабины или блока удалённого управления, которыми вы будете производить манипуляции.
     * Блок управления краном - кабина пилота или блок удалённого управления. 
     * Советую ставить блок удалённого управления, для того, чтобы была возможность управлять краном не находять в кабине.
     * Если хочется управлять из кабины, можно назначить в быстрое меню кнопку для взятие по контроль блок удалённого управления и будет иллюзия управления из кабины
     * Все требуемые для скрипта блоки будут оканчиваться на переменную shipName, которая обозначает название корабля или станции, к которой привязан кран, она настраивается ниже.
     * 
     * Блок управления краном: 
     * Remote Control shipName
     * 
     * Ротор движения крана вокруг своей оси:
     * Rotor shipName
     * Управление мышкой влево/вправо
     * 
     * Hinge, которым управляет наклон головки крана вверх вниз:
     * Hinge shipName
     * Управление мышкой вверх/вниз
     * 
     * Поршень движения влево/вправо:
     * Piston X shipName
     * Управление A/D
     * 
     * Поршень движения вперёд/назад:
     * Piston Y shipName
     * Управление W/S
     * 
     * Поршень движения вверх/вниз:
     * Piston Z shipName
     * Управление Space/C
     * 
     * Поршень движения базы крана (по умолчанию влево/вправо, но можно использовать как угодно):
     * Piston Base shipName
     * Управление Q/E
     * 
     * Если поршни или роторы двигаются в неверном управлении инвертируйте значение переменных, начинающихся со слова inver ниже, найдя требуемый объект
     * rotationCoef - скорость вращения крана вокруг своей оси и вверх вниз по движению мыши (чем больше значение, тем быстрее движение)
     * pistonVelocity - скорость движения поршней в системе от 0 до 5
     */

    String shipName = "Crain";

    IMyShipController shipController;
    IMyMotorStator rotor; // Ротор вращения крана вокруг своей оси
    IMyMotorStator hinge; // Поворот головки крана вверх вниз
    IMyPistonBase pistonX; // Влево/Вправо по умолчанию поршень выдвигается вправо
    IMyPistonBase pistonY; // Вперёд/назад по умолчанию поршень выдвигается вперёд
    IMyPistonBase pistonZ; // Вверх/Вниз по умолчанию поршень выдвигается вверх
    IMyPistonBase pistonBase;  // Вслево/Вправо по умолчанию поршень выдвигается вправо
    IMyTextSurface screen = null; // Экран для отладки

    float rotationCoef = 0.1F; // Скорость вращения вокруг своей оси и вверх вниз
    float pistonVelocity = 2F; // Скорость движения поршней (0 - 5)
    int invertRotor = 1; // 1 или -1 в зависимости от того правильно ли крутится кран вокруг своей оси
    int invertHinge = 1; // 1 или -1 в зависимости от того правильно ли вращается вверх вниз голова крана
    int invertX = 1; // 1 если поршень вправо/влево направлен вправо, иначе -1
    int invertY = -1; // 1 если поршень вперёд/назад направлен вперёд, иначе -1
    int invertZ = 1; // 1 если поршень вверх вниз направлен вверх, иначе -1
    int invertBase = 1; // 1 если поршень двигающий основание базы направлен влево, иначе -1

    public Program()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update1;

        screen = GridTerminalSystem.GetBlockWithName("LCD Panel " + shipName) as IMyTextSurface;
        if (screen != null)
        {
            screen.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            screen.FontSize = 1f;
        }
        else
        {
            Echo("Экран не найден");
        }

        shipController = GridTerminalSystem.GetBlockWithName("Remote Control " + shipName) as IMyShipController;
        if (shipController != null)
        {
        }
        else
        {
            Echo("Управление не найдено");
        }

        pistonX = getPiston("Piston X " + shipName);

        pistonY = getPiston("Piston Y " + shipName);

        pistonZ = getPiston("Piston Z " + shipName);

        pistonBase = getPiston("Piston Base " + shipName);

        rotor = getRotor("Rotor " + shipName);

        hinge = getRotor("Hinge " + shipName);
    }

    public void Main(string args)
    {
        StringBuilder message = new StringBuilder();
        message.Append("Статус: ");

        if (isPlayerControl(shipController))
        {
            message.Append("Корабль под управлением\n");
            Vector3 moveVector = shipController.MoveIndicator;
            Vector2 rotation = shipController.RotationIndicator;
            double roll = shipController.RollIndicator;

            movePiston(pistonY, moveVector.Z, invertY, message, "W|", "S|");
            movePiston(pistonX, moveVector.X, invertX, message, "A|", "D|");
            movePiston(pistonZ, moveVector.Y, invertZ, message, "C|", "Space|");
            message.Append("\n");

            movePiston(pistonBase, roll, invertBase, message, "Roll " + roll + "\n", "Roll " + roll + "\n");

            moveRotor(hinge, rotation.X, invertHinge, message, "Вверх/Вниз " + rotation.X + "\n");
            moveRotor(rotor, rotation.Y, invertRotor, message, "Влево/Вправо" + rotation.Y + "\n");
        }
        else
        {
            message.Append("Корабль без управления\n");
            stopPiston(pistonX);
            stopPiston(pistonY);
            stopPiston(pistonZ);
            stopPiston(pistonBase);

            stopRotor(rotor);
            stopRotor(hinge);
        }

        if (screen != null)
        {
            screen.WriteText(message);
        }
    }

    public void movePiston(IMyPistonBase piston, double direction, int invert, StringBuilder message, String message1, String message2)
    {
        if (piston != null)
        {
            if (direction != 0)
            {
                if (direction == -1)
                {
                    message.Append(message1);
                    piston.Velocity = -invert * pistonVelocity;
                }
                else
                {
                    message.Append(message2);
                    piston.Velocity = invert * pistonVelocity;
                }
            }
            else
            {
                piston.Velocity = 0;
            }
        }
    }

    public void moveRotor(IMyMotorStator rotor, double direction, int invert, StringBuilder message, String messageStr)
    {
        if (rotor != null)
        {
            if (direction != 0)
            {
                message.Append(messageStr);
                rotor.TargetVelocityRPM = invert * (float) direction * rotationCoef;
            }
            else
            {
                rotor.TargetVelocityRPM = 0;
            }
        }
    }

    public void stopPiston(IMyPistonBase piston)
    {
        if (piston != null)
        {
            piston.Velocity = 0;
        }
    }

    public void stopRotor(IMyMotorStator rotor)
    {
        if (rotor != null)
        {
            rotor.TargetVelocityRPM = 0;
        }
    }

    public IMyPistonBase getPiston(String name)
    {
        IMyPistonBase piston = GridTerminalSystem.GetBlockWithName(name) as IMyPistonBase;
        if (piston != null)
        {
            piston.Velocity = 0;
        }
        else
        {
            Echo(name + " не найден");
        }
        return piston;
    }

    public IMyMotorStator getRotor(String name)
    {
        IMyMotorStator rotor = GridTerminalSystem.GetBlockWithName(name) as IMyMotorStator;
        if (rotor != null)
        {
            rotor.TargetVelocityRPM = 0;
        }
        else
        {
            Echo(name + " не найден");
        }
        return rotor;
    }

    public bool isPlayerControl(IMyShipController shipController)
    {
        Vector3 move = shipController.MoveIndicator;
        Vector2 rotation = shipController.RotationIndicator;
        double roll = shipController.RollIndicator;

        return (move.X != 0) || (move.Y != 0) || (move.Z != 0) || (rotation.X != 0) || (rotation.Y != 0) || (roll != 0);
    }

    public void Save()
    {
    }
    // КОНЕЦ СКРИПТА
}