using System;
using UnityEngine;
using System.Collections.Generic;
using AI.Movement;
using AI.Formation;
using AI.Formation.Patterns;

namespace AI.Blackboard.Experts
{
    // Experto de la Escena 2 que gestiona el movimiento de la formacion en dos fases:
    // Fase 1 - rompe la formacion y manda a todos los agentes al punto del clic
    // Fase 2 - pasado reformDelay segundos, reactiva la formacion con el nuevo centro
    public class FormationMoveExpert : Expert
    {
        private readonly List<AgentMovement> agents;
        private readonly FormationManager formationManager;
        private readonly FormationPattern[] patterns;
        private readonly float reformDelay;

        // Flags que controlan en que fase del comportamiento se encuentra el experto
        private bool executed = false;
        private bool reforming = false;
        private float reformTimer = 0f;
        private int currentPatternIndex = 0;

        public FormationMoveExpert(
            List<AgentMovement> agents,
            FormationManager formationManager,
            FormationPattern[] patterns,
            float reformDelay)
        {
            this.agents = agents;
            this.formationManager = formationManager;
            this.patterns = patterns;
            this.reformDelay = reformDelay;
        }

        // El arbitro llama a este metodo para saber cuanto quiere actuar este experto
        // Devuelve 0.9 cuando hay un objetivo pendiente en la pizarra (fase 1)
        // Devuelve 0.5 mientras espera para reformar (fase 2)
        // Devuelve 0 cuando no hay nada que hacer
        public override float GetInsistence(BlackboardSystem blackboard)
        {
            // Lee el patron activo desde la pizarra para saber cual mover despues
            if (blackboard.HasKey("patternIndex"))
                currentPatternIndex = blackboard.GetValue<int>("patternIndex");

            if (blackboard.HasKey("target") && !executed)
                return 0.9f;

            if (reforming)
                return 0.5f;

            return 0f;
        }

        // El arbitro llama a este metodo cuando este experto gana la insistencia mas alta
        // Devuelve un array de acciones que se ejecutan en el Update del controlador de escena
        public override Action[] Run(BlackboardSystem blackboard)
        {
            // FASE 1: se ejecuta una sola vez cuando llega un nuevo objetivo a la pizarra
            if (blackboard.HasKey("target") && !executed)
            {
                Vector3 target = blackboard.GetValue<Vector3>("target");

                // Marcar como ejecutado para no repetir la fase 1
                // Iniciar el temporizador de espera antes de reformar
                executed = true;
                reforming = true;
                reformTimer = reformDelay;

                return new Action[]
                {
                    () =>
                    {
                        // Desactiva la formacion para que los agentes se muevan libremente
                        formationManager.SetActive(false);

                        // Cancela la patrulla de cada agente y los dirige al objetivo comun
                        foreach (AgentMovement agent in agents)
                        {
                            agent.ResetPatrol();
                            agent.SetTarget(target);
                        }
                    }
                };
            }

            // FASE 2: cuenta el tiempo y reforma cuando el temporizador llega a cero
            if (reforming)
            {
                // El temporizador se descuenta cada frame dentro de Run
                // porque GetInsistence devuelve 0.5 y el arbitro sigue llamando a Run
                reformTimer -= Time.deltaTime;

                if (reformTimer <= 0f)
                {
                    reforming = false;
                    executed = false;

                    Vector3 newCenter = blackboard.GetValue<Vector3>("target");
                    newCenter.y = 0f;

                    return new Action[]
                    {
                        () =>
                        {
                            // Mueve el ancla del patron activo al nuevo centro
                            // para que la formacion se reconstituya alrededor del punto del clic
                            switch (currentPatternIndex)
                            {
                                case 0: (patterns[0] as CirclePattern)?.SetCenter(newCenter); break;
                                case 1: (patterns[1] as VPattern)?.SetAnchor(newCenter);      break;
                                case 2: (patterns[2] as LinePattern)?.SetAnchor(newCenter);   break;
                            }

                            // Limpia el objetivo de la pizarra y reactiva la formacion
                            blackboard.RemoveData("target");
                            formationManager.SetActive(true);

                            // Cancela la patrulla para que cada agente vaya a su nuevo slot
                            foreach (AgentMovement agent in agents)
                                agent.ResetPatrol();
                        }
                    };
                }
            }

            return null;
        }
    }
}