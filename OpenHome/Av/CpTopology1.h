#ifndef HEADER_TOPOLOGY1
#define HEADER_TOPOLOGY1

#include <OpenHome/OhNetTypes.h>
#include <OpenHome/Private/Fifo.h>
#include <OpenHome/Private/Thread.h>
#include <OpenHome/Net/Core/CpDeviceUpnp.h>
#include <OpenHome/Net/Core/FunctorCpDevice.h>

namespace OpenHome {
    namespace Net {
        class CpStack;
    } // namespace Net
namespace Av {

class ICpTopology1Handler
{
public:
    virtual void ProductAdded(Net::CpDevice& aDevice) = 0;
    virtual void ProductRemoved(Net::CpDevice& aDevice) = 0;
    ~ICpTopology1Handler() {}
};

typedef void (ICpTopology1Handler::*ICpTopology1HandlerFunction)(Net::CpDevice&);

class CpTopology1Job
{
public:
    CpTopology1Job(ICpTopology1Handler& aHandler);
    void Set(Net::CpDevice& aDevice, ICpTopology1HandlerFunction aFunction);
    virtual void Execute();
	virtual ~CpTopology1Job() {}
private:
    ICpTopology1Handler* iHandler;
    Net::CpDevice* iDevice;
    ICpTopology1HandlerFunction iFunction;
};

class CpTopology1
{
    static const TUint kMaxJobCount = 20;
    
public:
    CpTopology1(Net::CpStack& aCpStack, ICpTopology1Handler& aHandler);
    
    void Refresh();
    
    virtual ~CpTopology1();
    
private:
    void ProductAdded(Net::CpDevice& aDevice);
    void ProductRemoved(Net::CpDevice& aDevice);

    void Run();
    
private:
    Net::CpDeviceList* iDeviceListProduct;
    Fifo<CpTopology1Job*> iFree;
    Fifo<CpTopology1Job*> iReady;
    ThreadFunctor* iThread;
};

} // namespace Av
} // namespace OpenHome

#endif // HEADER_TOPOLOGY1
