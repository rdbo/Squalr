﻿using Anathema.MemoryManagement;
using Anathema.MemoryManagement.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace Anathema
{
    class FiniteStateBuilder : IFiniteStateBuilderModel
    {
        // User controlled variables
        private FiniteStateMachine FiniteStateMachine;
        public event FiniteStateBuilderEventHandler EventUpdateDisplay;

        private ConstraintsEnum ValueConstraintSelection;
        private FiniteState DraggedState;
        private FiniteState TransitionPendingState;
        private FiniteState MousedOverState;
        private Point[] SelectionLine;

        private Int32 StateRadius;
        private Int32 StateEdgeSize;

        private dynamic CurrentValue;

        public FiniteStateBuilder()
        {
            FiniteStateMachine = new FiniteStateMachine();
        }

        public void SetElementType(Type ElementType)
        {
            FiniteStateMachine.SetElementType(ElementType);
        }

        public Type GetElementType()
        {
            return FiniteStateMachine.GetElementType();
        }

        public void SetCurrentValue(dynamic CurrentValue)
        {
            this.CurrentValue = CurrentValue;
        }

        public FiniteState GetMousedOverState()
        {
            return MousedOverState;
        }

        public Point[] GetSelectionLine()
        {
            return SelectionLine;
        }

        public void SetCurrentValueConstraint(ConstraintsEnum ValueConstraintSelection)
        {
            this.ValueConstraintSelection = ValueConstraintSelection;
        }

        public void SetFiniteStateMachine(FiniteStateMachine FiniteStateMachine)
        {
            this.FiniteStateMachine = FiniteStateMachine;
        }

        public void SetStateRadius(Int32 StateRadius)
        {
            this.StateRadius = StateRadius;
        }

        public void SetStateEdgeSize(Int32 StateEdgeSize)
        {
            this.StateEdgeSize = StateEdgeSize;
        }
        public Boolean IsStateAtPoint(Point Location)
        {
            if (FiniteStateMachine.GetStateUnderPoint(Location, StateRadius) != null)
                return true;
            return false;
        }

        public void SetStateEvent(Point Location, FiniteState.StateEventEnum StateEvent)
        {
            FiniteStateMachine.GetStateUnderPoint(Location, StateRadius).SetStateEvent(StateEvent);
        }

        public Boolean IsStateAtPointStartState(Point Location)
        {
            if (FiniteStateMachine.GetStateUnderPoint(Location, StateRadius) == FiniteStateMachine.GetStartState())
                return true;
            return false;
        }

        public void BeginAction(Point Location)
        {
            FiniteState State;

            // Handle edge selection
            State = FiniteStateMachine.GetEdgeUnderPoint(Location, StateRadius, StateEdgeSize);
            if (State != null)
            {
                TransitionPendingState = State;
                UpdateDisplay();
                return;
            }

            // Handle state selection (drag)
            State = FiniteStateMachine.GetStateUnderPoint(Location, StateRadius);
            if (State != null)
            {
                DraggedState = State;
                UpdateDisplay();
                return;
            }

            // Handle state creation
            FiniteStateMachine.AddNewState(Location);
            UpdateDisplay();
        }

        public void UpdateAction(Point Location)
        {
            // Update transition line dragging
            if (TransitionPendingState != null)
            {
                SelectionLine = new Point[2];
                SelectionLine[0] = TransitionPendingState.GetEdgePoint(Location, StateRadius);
                SelectionLine[1] = Location;
                UpdateDisplay();
                return;
            }

            // Update drag
            if (DraggedState != null)
            {
                DraggedState.Location = Location;
                UpdateDisplay();
                return;
            }

            MousedOverState = FiniteStateMachine.GetEdgeUnderPoint(Location, StateRadius, StateEdgeSize);
            UpdateDisplay();
        }

        public void FinishAction(Point Location)
        {
            // Update transition line dragging
            if (TransitionPendingState != null)
            {
                // Add a transition if possible
                FiniteState DestinationState = FiniteStateMachine.GetStateUnderPoint(Location, StateRadius);
                if (DestinationState != null && DestinationState != TransitionPendingState)
                {
                    ScanConstraint TransitionConstraint = new ScanConstraint(ValueConstraintSelection, CurrentValue);

                    if (!TransitionPendingState.ContainsDestionationState(DestinationState))
                        TransitionPendingState.AddTransition(TransitionConstraint, DestinationState);
                }
                SelectionLine = null;
                TransitionPendingState = null;
                UpdateDisplay();
                return;
            }

            // Update drag
            if (DraggedState != null)
            {
                DraggedState = null;
                UpdateDisplay();
                return;
            }
        }

        public void SetToStartState(Point Location)
        {
            FiniteState StateUnderPoint = FiniteStateMachine.GetStateUnderPoint(Location, StateRadius);
            if (StateUnderPoint == null)
                return;

            FiniteStateMachine.SetStartState(StateUnderPoint);
        }

        public FiniteState.StateEventEnum GetStateEventAtPoint(Point Location)
        {
            FiniteState StateUnderPoint = FiniteStateMachine.GetStateUnderPoint(Location, StateRadius);
            if (StateUnderPoint == null)
                return FiniteState.StateEventEnum.None;

            return StateUnderPoint.GetStateEvent();
        }

        public void DeleteAtPoint(Point Location)
        {
            FiniteState StateUnderPoint = FiniteStateMachine.GetStateUnderPoint(Location, StateRadius);
            if (StateUnderPoint == null)
                return;

            FiniteStateMachine.DeleteState(StateUnderPoint);

            UpdateDisplay();
        }

        public FiniteStateMachine GetFiniteStateMachine()
        {
            return FiniteStateMachine;
        }

        private void UpdateDisplay()
        {
            FiniteStateBuilderEventArgs FilterFSMEventArgs = new FiniteStateBuilderEventArgs();
            EventUpdateDisplay(this, FilterFSMEventArgs);
        }
    }
}