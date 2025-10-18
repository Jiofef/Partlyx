using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIServices.Interfaces
{
    public interface IVMFileService
    {
        bool IsChangesSaved { get; }

        Task NewFileAsync();
        Task<bool> SaveProjectAsync();
        Task<bool> SaveProjectAsAsync();
        Task OpenProjectAsync();
        Task DeleteWorkingDBAsync();
    }
}
