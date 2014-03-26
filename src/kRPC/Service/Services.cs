using System;
using System.Collections.Generic;
using System.Reflection;
using Google.ProtocolBuffers;
using KRPC.Schema.KRPC;
using KRPC.Service.Scanner;
using KRPC.Utils;

namespace KRPC.Service
{
    class Services
    {
        public IDictionary<string, ServiceSignature> Signatures { get; private set; }

        static Services instance;

        public static Services Instance {
            get {
                if (instance == null)
                    instance = new Services ();
                return instance;
            }
        }

        /// <summary>
        /// Create a Services instance. Scans the loaded assemblies for services, procedures etc.
        /// </summary>
        Services ()
        {
            Signatures = Scanner.Scanner.GetServices ();
        }

        /// <summary>
        /// Executes the given request and returns a response builder with the relevant
        /// fields populated. Throws RPCException if processing the request fails.
        /// </summary>
        public Response.Builder HandleRequest (Request request)
        {
            // Get the service definition
            if (!Signatures.ContainsKey (request.Service))
                throw new RPCException ("Service " + request.Service + " not found");
            var service = Signatures [request.Service];

            // Get the procedure definition
            if (!service.Procedures.ContainsKey (request.Procedure))
                throw new RPCException ("Procedure " + request.Procedure + " not found, " +
                "in Service " + request.Service);
            var procedure = service.Procedures [request.Procedure];

            // Invoke the procedure
            object[] arguments = DecodeArguments (procedure, request.ArgumentsList);
            object returnValue;
            try {
                returnValue = procedure.Handler.Invoke (arguments);
            } catch (TargetInvocationException e) {
                throw new RPCException ("Procedure '" + procedure.FullyQualifiedName + "' threw an exception. " +
                e.InnerException.GetType () + ": " + e.InnerException.Message);
            }
            var responseBuilder = Response.CreateBuilder ();
            if (procedure.HasReturnType) {
                responseBuilder.ReturnValue = EncodeReturnValue (procedure, returnValue);
            }
            return responseBuilder;
        }

        /// <summary>
        /// Decode the arguments for a procedure from a serialized request
        /// </summary>
        object[] DecodeArguments (ProcedureSignature procedure, IList<Schema.KRPC.Argument> arguments)
        {
            // Rearrange argument values
            var argumentValues = new ByteString [procedure.Parameters.Count];
            foreach (var argument in arguments)
                argumentValues [argument.Position] = argument.Value;

            var decodedArgumentValues = new object[procedure.Parameters.Count];
            for (int i = 0; i < procedure.Parameters.Count; i++) {
                var type = procedure.Parameters [i].Type;
                var value = argumentValues [i];
                if (value == null) {
                    // Handle default arguments
                    if (!procedure.Parameters [i].HasDefaultArgument)
                        throw new RPCException ("Argument not specified for parameter " + procedure.Parameters [i].Name + " in " + procedure.FullyQualifiedName + ". ");
                    decodedArgumentValues [i] = Type.Missing;
                } else {
                    // Decode argument
                    try {
                        if (TypeUtils.IsAClassType (type)) {
                            decodedArgumentValues [i] = ObjectStore.Instance.GetInstance ((ulong)ProtocolBuffers.ReadValue (value, typeof(ulong)));
                        } else if (ProtocolBuffers.IsAMessageType (type)) {
                            var builder = procedure.ParameterBuilders [i];
                            decodedArgumentValues [i] = builder.WeakMergeFrom (value).WeakBuild ();
                        } else if (ProtocolBuffers.IsAnEnumType (type) || TypeUtils.IsAnEnumType (type)) {
                            // TODO: Assumes it's underlying type is int
                            decodedArgumentValues [i] = ProtocolBuffers.ReadValue (value, typeof(int));
                        } else {
                            decodedArgumentValues [i] = ProtocolBuffers.ReadValue (value, type);
                        }
                    } catch (Exception e) {
                        throw new RPCException (
                            "Failed to decode argument for parameter " + procedure.Parameters [i].Name + " in " + procedure.FullyQualifiedName + ". " +
                            "Expected an argument of type " + ProtocolBuffers.GetTypeName (type) + ". " +
                            e.GetType ().Name + ": " + e.Message);
                    }
                }
            }
            return decodedArgumentValues;
        }

        /// <summary>
        /// Encodes the value returned by a procedure handler into a ByteString
        /// </summary>
        ByteString EncodeReturnValue (ProcedureSignature procedure, object returnValue)
        {
            // Check the return value is missing
            if (returnValue == null && !TypeUtils.IsAClassType (procedure.ReturnType)) {
                throw new RPCException (
                    procedure.FullyQualifiedName + " returned null. " +
                    "Expected an object of type " + procedure.ReturnType);
            }

            // Check if the return value is of a valid type
            if (!TypeUtils.IsAValidType (procedure.ReturnType)) {
                throw new RPCException (
                    procedure.FullyQualifiedName + " returned an object of an invalid type. " +
                    "Expected " + procedure.ReturnType + "; got " + returnValue.GetType ());
            }

            // Encode it as a ByteString
            if (TypeUtils.IsAClassType (procedure.ReturnType))
                return ProtocolBuffers.WriteValue (ObjectStore.Instance.AddInstance (returnValue), typeof(ulong));
            else if (ProtocolBuffers.IsAMessageType (procedure.ReturnType))
                return ProtocolBuffers.WriteMessage (returnValue as IMessage);
            else if (ProtocolBuffers.IsAnEnumType (procedure.ReturnType) || TypeUtils.IsAnEnumType (procedure.ReturnType)) {
                // TODO: Assumes it's underlying type is int
                return ProtocolBuffers.WriteValue ((int) returnValue, typeof(int));
            } else
                return ProtocolBuffers.WriteValue (returnValue, procedure.ReturnType);
        }
    }
}

