using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace BrokenEvent.ILStrip
{
    internal class KieronsAssemblyResolver : IAssemblyResolver
    {
        private DefaultAssemblyResolver defaultAssemblyResolver;

        private ConcurrentDictionary<string, AssemblyDefinition> _cachedAssemblies;

        public KieronsAssemblyResolver(DefaultAssemblyResolver defaultAssemblyResolver)
        {
            this.defaultAssemblyResolver = defaultAssemblyResolver;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            try
            {
                var ret =  resolveUsingCache(name);
                if (ret == null)
                    return this.defaultAssemblyResolver.Resolve(name);
                else return ret;

            }
            catch(AssemblyResolutionException e)
            {
                return this.defaultAssemblyResolver.Resolve(name);
            }
        }

        private AssemblyDefinition resolveUsingCache(AssemblyNameReference name)
        {
            if (_cachedAssemblies == null) _cachedAssemblies = new ConcurrentDictionary<string, AssemblyDefinition>();

            if (!_cachedAssemblies.ContainsKey(name.FullName))
            {
                var assemblyFQN = findAssembly(name);
                if (!string.IsNullOrEmpty(assemblyFQN))
                {
                    // locate and load the type from the alt folders
                    var def = AssemblyDefinition.ReadAssembly(assemblyFQN, new ReaderParameters() { AssemblyResolver = this });

                    _cachedAssemblies.TryAdd(name.FullName, def);
                }
            }

            AssemblyDefinition ret = null;
            if( _cachedAssemblies.TryGetValue(name.FullName, out ret))
            {
                return ret;
            }
            return null;

        }

        private string findAssembly(AssemblyNameReference name)
        {
            var altFolders = new[] { @"D:\code\firefly\Northwind3\Northwind\bin\Debug" };

            foreach(var f in altFolders)
            {
                var filepath = Path.Combine(f, name.Name + ".dll");
                if (File.Exists(filepath)) return filepath;
            }

            return string.Empty;

        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return this.defaultAssemblyResolver.Resolve(name, parameters);
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return this.defaultAssemblyResolver.Resolve(fullName);
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}