using System;
using System.Linq;
using dk.QA;
using UnityEngine;

namespace dk
{
    public class QaCommandContainer_Ingame : QaCommandContainer
    {
        protected override void CreateQaCommands()
        {
            CreateKeyCodeCommand(KeyCode.Space, "게임속도 x0", () => ChangeGameSpeed(0.0f));
            CreateKeyCodeCommand(KeyCode.Alpha1, "게임속도 x1", () => ChangeGameSpeed(1.0f));
            CreateKeyCodeCommand(KeyCode.Alpha2, "게임속도 x2", () => ChangeGameSpeed(2.0f));
            CreateKeyCodeCommand(KeyCode.Alpha3, "게임속도 x4", () => ChangeGameSpeed(4.0f));
            CreateKeyCodeCommand(KeyCode.Alpha4, "게임속도 x8", () => ChangeGameSpeed(8.0f));

            


            CreateButtonCommand("정지", () =>
            {
                Debug.Log("!");
            });


        }

        private void ChangeGameSpeed(float speed)
        {
            Time.timeScale = speed;
        }
    }
}
