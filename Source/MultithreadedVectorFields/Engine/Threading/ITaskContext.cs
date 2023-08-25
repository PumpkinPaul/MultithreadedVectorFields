//Code from Jon Watte
//http://www.enchantedage.com/
//http://xboxforums.create.msdn.com/forums/p/16153/84662.aspx

namespace MultithreadedVectorFields.Engine.Threading;

public interface ITaskContext
{
    void Init(ITask task);
}