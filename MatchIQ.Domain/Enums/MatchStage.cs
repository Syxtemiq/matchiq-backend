namespace MatchIQ.Domain.Enums;

public enum MatchStage
{
    Matched,        // candidato apareció en el ranking
    TestSent,       // la empresa le envió el test
    TestCompleted,  // el candidato respondió
    Selected,       // la empresa lo seleccionó
    Rejected        // descartado en algún punto
}
