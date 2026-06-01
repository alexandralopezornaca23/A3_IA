using System;
using UnityEngine;
using System.Collections.Generic;
using AI.Movement;
using AI.Formation;
using AI.Formation.Patterns;

namespace AI.Blackboard.Experts
{
    // Experto que:
    // 1. Rompe la formación y manda todos los agentes al punto del clic
    // 2. Tras reformDelay segundos, reactiva la formación con el nuevo centro
    public class FormationMoveExpert : Expert
    {
        private readonly List<AgentMovement> agents;
        private readonly FormationManager formationManager;
        private readonly FormationPattern[] patterns;
        private readonly float reformDelay;

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

        public override float GetInsistence(BlackboardSystem blackboard)
        {
            // Actualizar el índice del patrón activo desde la pizarra
            if (blackboard.HasKey("patternIndex"))
                currentPatternIndex = blackboard.GetValue<int>("patternIndex");

            // Si hay target pendiente y no se ha ejecutado aún
            if (blackboard.HasKey("target") && !executed)
                return 0.9f;

            // Si está en fase de espera antes de reformar
            if (reforming)
                return 0.5f;

            return 0f;
        }

        public override Action[] Run(BlackboardSystem blackboard)
        {
            // FASE 1: romper formación y mandar a todos al objetivo
            if (blackboard.HasKey("target") && !executed)
            {
                Vector3 target = blackboard.GetValue<Vector3>("target");
                executed = true;
                reforming = true;
                reformTimer = reformDelay;

                return new Action[]
                {
                    () =>
                    {
                        // Romper formación
                        formationManager.SetActive(false);

                        // Resetear patrulla y mandar al objetivo
                        foreach (AgentMovement agent in agents)
                        {
                            agent.ResetPatrol();
                            agent.SetTarget(target);
                        }
                    }
                };
            }

            // FASE 2: contar el tiempo y reformar cuando acabe
            if (reforming)
            {
                reformTimer -= Time.deltaTime;

                if (reformTimer <= 0f)
                {
                    reforming = false;
                    executed = false;

                    // Leer el target antes de borrarlo
                    Vector3 newCenter = blackboard.GetValue<Vector3>("target");
                    newCenter.y = 0f;

                    return new Action[]
                    {
                        () =>
                        {
                            // Mover el ancla del patrón activo al nuevo centro
                            switch (currentPatternIndex)
                            {
                                case 0: (patterns[0] as CirclePattern)?.SetCenter(newCenter); break;
                                case 1: (patterns[1] as VPattern)?.SetAnchor(newCenter);      break;
                                case 2: (patterns[2] as LinePattern)?.SetAnchor(newCenter);   break;
                            }

                            // Limpiar la pizarra y reactivar formación
                            blackboard.RemoveData("target");
                            formationManager.SetActive(true);

                            // Resetear patrulla para que vayan a los nuevos slots
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